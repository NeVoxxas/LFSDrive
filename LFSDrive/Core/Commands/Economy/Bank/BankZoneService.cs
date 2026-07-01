using LfsCruise.Core.Players;
using LfsCruise.Core.UI.Menu;
using LfsCruise.Core.World;

namespace LfsCruise.Core.Economy.Bank;

public sealed class BankZoneService
{
    private const double MaxStopSpeed = 1.0;

    private readonly ZoneManager _zoneManager;
    private readonly MenuManager _menuManager;
    private readonly HashSet<byte> _openedFor = new();

    public BankZoneService(ZoneManager zoneManager, MenuManager menuManager)
    {
        _zoneManager = zoneManager;
        _menuManager = menuManager;
    }

    public async Task OnPlayerMoveAsync(Player player, CancellationToken cancellationToken)
    {
        var inBankZone = _zoneManager.Zones.Any(z => z.Type == ZoneType.Bank && z.Contains(player));

        if (!inBankZone)
        {
            _openedFor.Remove(player.UCID);
            return;
        }

        if (_openedFor.Contains(player.UCID))
            return;

        if (player.Vehicle.Speed > MaxStopSpeed)
            return;

        _openedFor.Add(player.UCID);

        await _menuManager.OpenBankAsync(player, allowTransactions: true, showBackButton: false, cancellationToken);
    }

    public void RemovePlayer(byte ucid)
    {
        _openedFor.Remove(ucid);
    }
}