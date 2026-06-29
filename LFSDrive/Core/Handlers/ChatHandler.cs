using LfsCruise.Core.Commands;
using LfsCruise.Core.Players;
using LfsCruise.InSim.Packets;

namespace LfsCruise.InSim.Handlers;

public sealed class ChatHandler
{
    private readonly PlayerManager _playerManager;
    private readonly CommandManager _commandManager;

    public ChatHandler(PlayerManager playerManager, CommandManager commandManager)
    {
        _playerManager = playerManager;
        _commandManager = commandManager;
    }

    public async Task HandleAsync(MsoPacket packet, CancellationToken cancellationToken)
    {
        var player = _playerManager.Get(packet.UCID);

        if (player is null)
            return;

        await _commandManager.ExecuteAsync(player, packet.Text, cancellationToken);
    }
}