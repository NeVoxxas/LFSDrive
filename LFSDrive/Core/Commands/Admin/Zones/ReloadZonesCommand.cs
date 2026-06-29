using LfsCruise.Core.Players;
using LfsCruise.Core.World;

namespace LfsCruise.Core.Commands.Admin.Zones;

public sealed class ReloadZonesCommand : ICommand
{
    private readonly ZoneService _zoneService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public ReloadZonesCommand(
        ZoneService zoneService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _zoneService = zoneService;
        _sendMessage = sendMessage;
    }

    public string Name => "reloadzones";
    public string Description => "Reload all zones.";
    public bool AdminOnly => true;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        _zoneService.Reload();

        await _sendMessage(
            player.UCID,
            $"^2[ZONE] Reloaded ^7{_zoneService.GetAll().Count} ^2zones.",
            cancellationToken);
    }
}