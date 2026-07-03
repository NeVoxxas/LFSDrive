using LfsCruise.Core.Players;
using LfsCruise.Core.UI.Menu;
using LfsCruise.Core.World;

namespace LfsCruise.Core.Vehicles.Regitra;

public sealed class RegitraZoneService
{

    private const double MaxStopSpeed = 1.0;

    private readonly ZoneManager _zoneManager;
    private readonly MenuManager _menuManager;
    private readonly HashSet<byte> _openedFor = new();

    public RegitraZoneService(ZoneManager zoneManager, MenuManager menuManager)
    {
        _zoneManager = zoneManager;
        _menuManager = menuManager;
    }

    public async Task OnPlayerMoveAsync(Player player, CancellationToken cancellationToken)
    {
        var inRegitraZone = _zoneManager.Zones.Any(z => z.Type == ZoneType.Regitra && z.Contains(player));

        if(!inRegitraZone)
        {
            _openedFor.Remove(player.UCID);
            return;
        }

        if (_openedFor.Contains(player.UCID))
            return;

        if (player.Vehicle.Speed > MaxStopSpeed)
            return;

        _openedFor.Add(player.UCID);

        await _menuManager.OpenRegitraAsync(player, cancellationToken);
    }

    public void RemovePlayer(byte ucid)
    {
        _openedFor.Remove(ucid);
    }
}

