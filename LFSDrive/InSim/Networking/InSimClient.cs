using LfsCruise.Core;
using LfsCruise.Database;
using LfsCruise.Core.Commands;
using LfsCruise.Core.Events;
using LfsCruise.Core.Players;
using LfsCruise.InSim.Packets;
using LfsCruise.Utils;
using System.Net.Sockets;


namespace LfsCruise.InSim.Networking;

public sealed class InSimClient : IDisposable
{
    private readonly ServerConfig _config;
    private readonly PacketFactory _packetFactory;
    private readonly TcpClient _tcpClient;
    private readonly PlayerManager _playerManager = new();
    private NetworkStream? _networkStream;
    private readonly CommandManager _commandManager = new();
    private readonly EventBus _eventBus = new();
    private readonly DatabaseService _databaseService;



    public InSimClient(ServerConfig config, PacketFactory? packetFactory = null)
    {
        _config = config;
        _packetFactory = packetFactory ?? new PacketFactory();
        _tcpClient = new TcpClient();

        var databaseConfig = new DatabaseConfig();

        _databaseService = new DatabaseService(databaseConfig);

        _commandManager = new CommandManager();
        _commandManager.Register(new HelpCommand(SendMessageToConnectionAsync));
        _commandManager.Register(new IdCommand(SendMessageToConnectionAsync));

        _eventBus.Subscribe(
            new PlayerConnectedHandler(
                _playerManager,
                _databaseService,
                SendMessageToConnectionAsync
            )
        );
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _tcpClient.ConnectAsync(_config.Host, _config.Port, cancellationToken);
            _networkStream = _tcpClient.GetStream();

            Console.WriteLine($"TCP connected to LFS host {_config.Host}:{_config.Port}");

            var isiPacket = new IsiPacket
            {
                UDPPort = 0,
                Flags = 0,
                Prefix = (byte)_config.Prefix,
                Interval = checked((ushort)_config.Interval),
                Admin = _config.AdminPassword,
                IName = _config.InSimName
            }.ToArray();

            await SendPacketAsync(isiPacket, cancellationToken);
            Console.WriteLine("IS_ISI packet sent");

            await ReceiveLoopAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Shutdown requested. InSim client stopped.");
        }
        catch (SocketException exception)
        {
            Console.WriteLine($"Socket error: {exception.Message}");
        }
        catch (IOException exception)
        {
            Console.WriteLine($"I/O error: {exception.Message}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Unexpected error: {exception.Message}");
        }
    }

    public Task SendPacketAsync(byte[] packet)
    {
        return SendPacketAsync(packet, CancellationToken.None);
    }

    public async Task ReceiveLoopAsync(CancellationToken cancellationToken = default)
    {
        var stream = GetNetworkStream();
        var sizeBuffer = new byte[1];

        Console.WriteLine("Waiting for InSim packets...");

        while (!cancellationToken.IsCancellationRequested)
        {
            var read = await stream.ReadAsync(sizeBuffer, cancellationToken);
            if (read == 0)
            {
                Console.WriteLine("Connection closed by LFS host.");
                return;
            }

            var sizeByte = sizeBuffer[0];
            if (sizeByte == 0)
            {
                Console.WriteLine("Connection lost: received packet size 0.");
                return;
            }

            var size = sizeByte * 4;

            var packet = new byte[size];
            packet[0] = sizeByte;

            await ReadExactAsync(stream, packet.AsMemory(1, size - 1), cancellationToken);

            var inSimPacket = _packetFactory.Create(packet);
            WritePacketLog(inSimPacket);

            // Hostingo KeepAlive

            if (inSimPacket is TinyPacket tiny)
            {
                Console.WriteLine($"IS_TINY received | SubT: {tiny.SubT}");

                if (tiny.SubT == 0)
                {
                    await SendPacketAsync(tiny.ToArray(), cancellationToken);
                    Console.WriteLine("IS_TINY keep-alive replied");
                }

                continue;
            }

            if (inSimPacket is NcnPacket ncn)
            {
                await _eventBus.PublishAsync(ncn, cancellationToken);
            }

            if (inSimPacket is CnlPacket cnl)
            {
                _playerManager.Remove(cnl.UCID);

                Console.WriteLine("------------------------");
                Console.WriteLine("Player disconnected");
                Console.WriteLine($"UCID           : {cnl.UCID}");
                Console.WriteLine($"Reason         : {cnl.Reason}");
                Console.WriteLine($"Players online : {_playerManager.Count}");
                Console.WriteLine("------------------------");

                continue;
            }

            if (inSimPacket is MsoPacket mso)
            {
                var player = _playerManager.Get(mso.UCID);

                if (player is not null)
                {
                    await _commandManager.ExecuteAsync(player, mso.Text, cancellationToken);
                }

                continue;
            }

        }
    }


    private static void WritePacketLog(InSimPacket packet)
    {
        if (packet is VerPacket verPacket)
        {
            Console.WriteLine("------------------------");
            Console.WriteLine("LFS Connected");
            Console.WriteLine($"Version: {verPacket.Version}");
            Console.WriteLine($"Product: {verPacket.Product}");
            Console.WriteLine($"InSim Version: {verPacket.InSimVersion}");
            Console.WriteLine("------------------------");

            return;
        }

        if (packet is NcnPacket ncn)
        {
            Console.WriteLine("------------------------");
            Console.WriteLine("Player connected");
            Console.WriteLine($"UCID     : {ncn.UCID}");
            Console.WriteLine($"Username : {ncn.Username}");
            Console.WriteLine($"Name     : {ncn.PlayerName}");
            Console.WriteLine($"Admin    : {ncn.IsAdmin}");
            Console.WriteLine($"Total    : {ncn.Total}");
            Console.WriteLine("------------------------");
            return;
        }

        if (packet is UnknownPacket unknownPacket)
        {
            Console.WriteLine("Unknown packet:");
            Console.WriteLine($"Type: {unknownPacket.Type}");
            Console.WriteLine($"Size: {unknownPacket.Size}");
        }
    }

    private async Task SendPacketAsync(byte[] packet, CancellationToken cancellationToken)
    {
        var stream = GetNetworkStream();

        await stream.WriteAsync(packet, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public async Task SendMessageToConnectionAsync(byte ucid, string message, CancellationToken cancellationToken = default)
    {
        var packet = new MtcPacket
        {
            UCID = ucid,
            PLID = 0,
            Sound = 1,
            Text = message
        }.ToArray();

        await SendPacketAsync(packet, cancellationToken);
    }

    private static async Task ReadExactAsync(
        NetworkStream stream,
        Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        var totalBytesRead = 0;

        while (totalBytesRead < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(buffer[totalBytesRead..], cancellationToken);
            if (bytesRead == 0)
            {
                throw new IOException("Connection closed while reading an InSim packet.");
            }

            totalBytesRead += bytesRead;
        }
    }

    private NetworkStream GetNetworkStream()
    {
        return _networkStream ?? throw new InvalidOperationException("InSim client is not connected.");
    }

    public void Dispose()
    {
        _networkStream?.Dispose();
        _tcpClient.Dispose();
    }
}
