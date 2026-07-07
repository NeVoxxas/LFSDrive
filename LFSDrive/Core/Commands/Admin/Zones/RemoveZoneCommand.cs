using LfsCruise.Core.Players;
using LfsCruise.Core.World;

namespace LfsCruise.Core.Commands.Admin.Zones;

public sealed class RemoveZoneCommand : ICommand
{
    private readonly ZoneService _zoneService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public RemoveZoneCommand(
        ZoneService zoneService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _zoneService = zoneService;
        _sendMessage = sendMessage;
    }

    public string Name => "removezone";
    public string Description => "Removes a zone by name.";
    public bool AdminOnly => true;
    public AdminRank RequiredRank => AdminRank.Moderator;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 1)
        {
            await _sendMessage(player.UCID, "^1Usage: ^7!removezone <name>", cancellationToken);
            return;
        }

        var name = args[0];

        if (!_zoneService.RemoveZone(name))
        {
            await _sendMessage(player.UCID, $"^1Zone not found: ^7{name}", cancellationToken);
            return;
        }

        await _sendMessage(player.UCID, $"^2[ZONE] Removed: ^7{name}", cancellationToken);
    }
}