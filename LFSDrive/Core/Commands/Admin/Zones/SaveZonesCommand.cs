using LfsCruise.Core.Players;
using LfsCruise.Core.World;

namespace LfsCruise.Core.Commands.Admin.Zones;

public sealed class SaveZonesCommand : ICommand
{
    private readonly ZoneService _zoneService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public SaveZonesCommand(
        ZoneService zoneService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _zoneService = zoneService;
        _sendMessage = sendMessage;
    }

    public string Name => "savezones";
    public string Description => "Save all zones.";
    public bool AdminOnly => true;

    public AdminRank RequiredRank => AdminRank.Moderator;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        _zoneService.Save();

        await _sendMessage(
            player.UCID,
            $"^2[ZONE] Saved ^7{_zoneService.GetAll().Count} ^2zones.",
            cancellationToken);
    }
}