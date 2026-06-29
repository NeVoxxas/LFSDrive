using LfsCruise.Core;
using LfsCruise.Core.Commands;
using LfsCruise.Core.Economy;
using LfsCruise.Core.Events;
using LfsCruise.Core.Players;
using LfsCruise.Core.Progression;
using LfsCruise.Core.UI;
using LfsCruise.Core.UI.HUD;
using LfsCruise.Core.UI.Menu;
using LfsCruise.Core.Vehicles;
using LfsCruise.Core.Vehicles.Shop;
using LfsCruise.Core.World;
using LfsCruise.Database;
using LfsCruise.InSim.Enums;
using LfsCruise.InSim.Handlers;
using LfsCruise.InSim.Packets;
using LfsCruise.Utils;
using System.Net.Sockets;
using System.Numerics;


namespace LfsCruise.InSim.Networking;

public sealed class InSimClient : IDisposable
{
    private readonly ServerConfig _config;
    private readonly PacketFactory _packetFactory;
    private readonly TcpClient _tcpClient;

    private readonly PlayerManager _playerManager = new();
    private readonly EconomyService _economyService = new();
    private readonly ZoneService _zoneService;

    private NetworkStream? _networkStream;

    private readonly CommandManager _commandManager;

    private readonly EventBus _eventBus = new(); // Event

    private readonly DatabaseService _databaseService; // DB

    private readonly ZoneManager _zoneManager = new(); // Zonos

    private readonly ProgressionService _progressionService = new(); // Progresas

    private readonly VehicleOwnershipService _vehicleOwnershipService;

    private readonly VehicleShopService _vehicleShopService;

    private readonly HudManager _hudManager; // Hudas

    private readonly HudUpdateLoop _hudUpdateLoop;

    private readonly MenuManager _menuManager; // Hudas

    // HANDLERIAI

    private readonly ChatHandler _chatHandler;
    private readonly PitHandler _pitHandler;
    private readonly MciHandler _mciHandler;
    private readonly ConnectionHandler _connectionHandler;


    public InSimClient(ServerConfig config, PacketFactory? packetFactory = null)
    {
        _config = config;
        _packetFactory = packetFactory ?? new PacketFactory();
        _tcpClient = new TcpClient();

        var databaseConfig = new DatabaseConfig();

        _databaseService = new DatabaseService(databaseConfig);

        _vehicleOwnershipService = new VehicleOwnershipService(databaseConfig);

        _vehicleShopService = new VehicleShopService(new VehicleShopStorage());

        _zoneService = new ZoneService(_zoneManager, new ZoneStorage());
        _zoneService.Load();

        _commandManager = new CommandManager(SendMessageToConnectionAsync);
        CommandLoader.RegisterAll(
            _commandManager,
            _economyService,
            _zoneService,
            _progressionService,
            SendMessageToConnectionAsync);

        _eventBus.Subscribe(
            new PlayerConnectedHandler(
                _playerManager,
                _databaseService,
                SendMessageToConnectionAsync
            )
        );

        var hudRenderer = new HudRenderer(SendButtonAsync);

        _hudManager = new HudManager(
            hudRenderer,
            _progressionService);

        _hudUpdateLoop = new HudUpdateLoop(_playerManager, _hudManager);

        var menuRenderer = new MenuRenderer( SendButtonAsync, DeleteButtonRangeAsync);

        _menuManager = new MenuManager(menuRenderer, _vehicleShopService, _vehicleOwnershipService, _economyService, _databaseService, SendMessageToConnectionAsync);

        //HANDLERIAI

        _chatHandler = new ChatHandler(_playerManager, _commandManager);
        _pitHandler = new PitHandler(_playerManager, _economyService, _databaseService, SendMessageToConnectionAsync);
        _mciHandler = new MciHandler( _playerManager, _zoneManager, _progressionService);
        _connectionHandler = new ConnectionHandler(_playerManager, _databaseService, _eventBus, _hudManager, _vehicleOwnershipService, _vehicleShopService, SendMessageToConnectionAsync, SendHostCommandAsync);
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
                Flags = 0x20 | 0x0800,
                Prefix = (byte)_config.Prefix,
                Interval = checked((ushort)_config.Interval),
                Admin = _config.AdminPassword,
                IName = _config.InSimName
            }.ToArray();

            await SendPacketAsync(isiPacket, cancellationToken);
            Console.WriteLine("IS_ISI packet sent");

            _ = Task.Run(() => _hudUpdateLoop.RunAsync(cancellationToken), cancellationToken);
            Console.WriteLine("HUD loop started");

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

            // Gaunam jau prisijungusius, ir užkraunam InSim

            if (inSimPacket is VerPacket)
            {
                await RequestInitialStateAsync(cancellationToken);
                Console.WriteLine("Initial state requested.");
            }

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

            // Handleriai

            switch (inSimPacket)
            {
                case MsoPacket mso:
                    await _chatHandler.HandleAsync(mso, cancellationToken);
                    continue;

                case PitPacket pit:
                    await _pitHandler.HandleAsync(pit, cancellationToken);
                    continue;

                case MciPacket mci:
                    _mciHandler.Handle(mci);
                    continue;

                case NcnPacket ncn:
                    await _connectionHandler.HandleConnectedAsync(ncn, cancellationToken);
                    continue;

                case CnlPacket cnl:
                    await _connectionHandler.HandleDisconnectedAsync(cnl, cancellationToken);
                    continue;

                case NplPacket npl:

                    await _connectionHandler.HandleNewPlayerAsync(npl, cancellationToken);
                    //Console.WriteLine($"NPL CAR RAW: '{npl.CarName}' LEN={npl.CarName.Length}");
                    continue;
                case BtcPacket btc:
                    {
                        var player = _playerManager.Get(btc.UCID);

                        if (player is null)
                            continue;

                        switch (btc.ClickID)
                        {
                            case ClickIds.Hud.Menu:
                                await _menuManager.OpenMainMenuAsync(player, cancellationToken);
                                break;

                            case ClickIds.Menu.Close:
                                await _menuManager.CloseAsync(player, cancellationToken);
                                break;

                            default:
                                await _menuManager.HandleClickAsync(player, btc.ClickID, cancellationToken);
                                break;
                        }

                        continue;
                    }

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

    public async Task SendHostCommandAsync(
        string command,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"HOST CMD -> {command}");
        var packet = new MstPacket
        {
            Msg = command
        };

        await SendPacketAsync(packet.ToArray(), cancellationToken);
    }

    private static async Task ReadExactAsync( NetworkStream stream, Memory<byte> buffer, CancellationToken cancellationToken)
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

    public async Task RequestInitialStateAsync(CancellationToken cancellationToken)
    {
        await SendTinyAsync(1, 13, cancellationToken); // TINY_NCN
        await SendTinyAsync(2, 14, cancellationToken); // TINY_NPL
        await SendTinyAsync(3, 23, cancellationToken); // TINY_NCI
        await SendTinyAsync(4, 7, cancellationToken);  // TINY_SST
    }

    private async Task SendTinyAsync(byte reqI, byte subT, CancellationToken cancellationToken)
    {
        var packet = new byte[]
        {
        1, // Size: 4 bytes / 4
        3, // ISP_TINY
        reqI,
        subT
        };

        await SendPacketAsync(packet, cancellationToken);
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

    public async Task ClearButtonsAsync( byte ucid, CancellationToken cancellationToken = default)
    {
        var packet = new BfnPacket
        {
            SubT = (byte)BfnSubType.Clear,
            UCID = ucid
        };

        await SendPacketAsync(packet.ToArray(), cancellationToken);
    }

    public async Task SendButtonAsync(
    byte ucid,
    byte clickId,
    byte left,
    byte top,
    byte width,
    byte height,
    string text,
    CancellationToken cancellationToken = default)
    {
        var packet = new BtnPacket
        {
            UCID = ucid,
            ClickID = clickId,
            Inst = 0,
            BStyle = 0x20 | 0x40 | 0x08, // dark + light text
            TypeIn = 0,
            L = left,
            T = top,
            W = width,
            H = height,
            Text = text
        }.ToArray();

        //Console.WriteLine($"BTN -> UCID {ucid} ID {clickId} Text: {text}");
        await SendPacketAsync(packet, cancellationToken);
    }
    public async Task DeleteButtonRangeAsync(
        byte ucid,
        byte firstId,
        byte lastId,
        CancellationToken cancellationToken = default)
    {
        var packet = new BfnPacket
        {
            SubT = (byte)BfnSubType.DeleteButton,
            UCID = ucid,
            ClickID = firstId,
            ClickMax = lastId
        };

        await SendPacketAsync(packet.ToArray(), cancellationToken);
    }
}
