using LfsCruise.Core.Players;
using LfsCruise.Database;
using LfsCruise.InSim.Packets;

namespace LfsCruise.Core.Events;

public sealed class PlayerConnectedHandler
    : IEventHandler<NcnPacket>
{
    private readonly PlayerManager _playerManager;
    private readonly DatabaseService _databaseService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public PlayerConnectedHandler(
        PlayerManager playerManager,
        DatabaseService databaseService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _playerManager = playerManager;
        _databaseService = databaseService;
        _sendMessage = sendMessage;
    }

    public async Task HandleAsync(
        NcnPacket packet,
        CancellationToken cancellationToken)
    {
        var player = new Player
        {
            UCID = packet.UCID,
            Username = packet.Username,
            Nickname = packet.PlayerName,
            TotalConnections = packet.Total
        };

        player.Data = await _databaseService.GetOrCreatePlayerAsync(player, cancellationToken);
        player.AdminLevel = await _databaseService.GetAdminLevelAsync(player.Username, cancellationToken);

        _playerManager.Add(player);

        await _sendMessage(
            player.UCID,
            $"^2Zdrw Duxas! ^2Pinigai: ^2${player.Data.Money} ^7Banke: ^2${player.Data.Bank}",
            cancellationToken
        );

        Console.WriteLine($"Players online: {_playerManager.Count}");
    }
}