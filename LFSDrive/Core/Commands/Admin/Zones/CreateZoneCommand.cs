using LfsCruise.Core.Players;
using LfsCruise.Core.World;

namespace LfsCruise.Core.Commands.Admin.Zones;

public sealed class CreateZoneCommand : ICommand
{
    private readonly ZoneService _zoneService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public CreateZoneCommand(
        ZoneService zoneService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _zoneService = zoneService;
        _sendMessage = sendMessage;
    }

    public string Name => "createzone";
    public string Description => "Creates a zone at your current position.";
    public bool AdminOnly => true;
    public AdminRank RequiredRank => AdminRank.Moderator;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 3)
        {
            await _sendMessage(player.UCID, "^1Usage: ^7!createzone <type> <name> <radius>", cancellationToken);
            return;
        }

        if (!Enum.TryParse<ZoneType>(args[0], true, out var type))
        {
            await _sendMessage(player.UCID, "^1Invalid zone type.", cancellationToken);
            return;
        }

        var name = args[1];

        if (!double.TryParse(args[2], out var radius) || radius <= 0)
        {
            await _sendMessage(player.UCID, "^1Invalid radius.", cancellationToken);
            return;
        }

        var zone = _zoneService.CreateZone(
            type,
            name,
            player.Vehicle.X,
            player.Vehicle.Y,
            radius);

        await _sendMessage(
            player.UCID,
            $"^2[ZONE] Zone created: ^7{zone.Name} ^2Type: ^7{zone.Type} ^2R: ^7{zone.Radius}",
            cancellationToken);
    }
}