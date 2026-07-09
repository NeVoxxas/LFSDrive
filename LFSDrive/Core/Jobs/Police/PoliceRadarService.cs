using LfsCruise.Core.Players;
using LfsCruise.Core.UI;
using LfsCruise.Core.Vehicles.Tow;

namespace LfsCruise.Core.Jobs.Police;

public sealed class PoliceRadarService
{
    private const double RangeMeters = 100.0;
    private const int MaxRows = 3;

    private const byte RadarLeft = 88;
    private const byte RadarTop = 10;
    private const byte RadarWidth = 42;
    private const byte HeaderHeight = 5;
    private const byte RowHeight = 6;

    private readonly PlayerManager _playerManager;
    private readonly JobService _jobService;
    private readonly PursuitService _pursuitService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendButton;
    private readonly Func<byte, byte, byte, CancellationToken, Task> _deleteButtons;

    private readonly HashSet<byte> _activeOfficers = new();
    private readonly Dictionary<byte, List<byte>> _lastRowTargets = new();

    public PoliceRadarService(
        PlayerManager playerManager,
        JobService jobService,
        PursuitService pursuitService,
        Func<byte, string, CancellationToken, Task> sendMessage,
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendButton,
        Func<byte, byte, byte, CancellationToken, Task> deleteButtons)
    {
        _playerManager = playerManager;
        _jobService = jobService;
        _pursuitService = pursuitService;
        _sendMessage = sendMessage;
        _sendButton = sendButton;
        _deleteButtons = deleteButtons;
    }

    public async Task UpdateAsync(Player officer, CancellationToken cancellationToken)
    {
        var onDuty = _jobService.GetJob(officer) == JobType.Police;

        if (!onDuty)
        {
            if (_activeOfficers.Remove(officer.UCID))
            {
                await _deleteButtons(officer.UCID, ClickIds.Police.RadarBackground, ClickIds.Police.RadarRowEnd, cancellationToken);
                _lastRowTargets.Remove(officer.UCID);
            }

            return;
        }

        var nearby = _playerManager.Players
            .Where(p => p.UCID != officer.UCID)
            .Select(p => (Player: p, Distance: Distance(officer, p)))
            .Where(x => x.Distance <= RangeMeters)
            .OrderBy(x => x.Distance)
            .Take(MaxRows)
            .ToList();

        if (_activeOfficers.Add(officer.UCID))
        {
            await _sendButton(
                officer.UCID, ClickIds.Police.RadarBackground,
                RadarLeft, RadarTop, RadarWidth, HeaderHeight,
                "^3Radaras (100m)", cancellationToken);
        }

        var previousRowCount = _lastRowTargets.TryGetValue(officer.UCID, out var previousTargets)
            ? previousTargets.Count
            : 0;

        var rowClickId = ClickIds.Police.RadarRowStart;
        var top = (byte)(RadarTop + HeaderHeight);
        var rowTargets = new List<byte>();

        for (var i = 0; i < nearby.Count; i++)
        {
            var (target, distance) = nearby[i];
            var wantedTag = _pursuitService.IsWanted(target.UCID) ? " ^1[G]" : "";

            await _sendButton(
                officer.UCID, rowClickId, RadarLeft, top, RadarWidth, RowHeight,
                $"^7{target.Username} ^8{distance:0}m{wantedTag}", cancellationToken);

            rowTargets.Add(target.UCID);
            rowClickId++;
            top += RowHeight;
        }

        if (nearby.Count < previousRowCount)
        {
            var firstUnusedClickId = (byte)(ClickIds.Police.RadarRowStart + nearby.Count);
            await _deleteButtons(officer.UCID, firstUnusedClickId, ClickIds.Police.RadarRowEnd, cancellationToken);
        }

        _lastRowTargets[officer.UCID] = rowTargets;
    }

    // Grazina true, jei clickId priklauso radarui (nesvarbu, ar veiksmas pavyko).
    public async Task<bool> HandleClickAsync(Player officer, byte clickId, CancellationToken cancellationToken)
    {
        if (clickId < ClickIds.Police.RadarRowStart || clickId > ClickIds.Police.RadarRowEnd)
            return false;

        if (!_lastRowTargets.TryGetValue(officer.UCID, out var rowTargets))
            return true;

        var index = clickId - ClickIds.Police.RadarRowStart;

        if (index < 0 || index >= rowTargets.Count)
            return true;

        var target = _playerManager.Get(rowTargets[index]);

        if (target is null)
            return true;

        /*if (officer.Vehicle.Speed > TowService.MaxStopSpeedKmh || target.Vehicle.Speed > TowService.MaxStopSpeedKmh)
        {
            await _sendMessage(officer.UCID, "^1Abu turite buti sustoje, kad pradetum gaudyma.", cancellationToken);
            return true;
        }*/

        await _pursuitService.StartPursuitAsync(officer, target, cancellationToken);

        return true;
    }

    public void RemovePlayer(byte ucid)
    {
        _activeOfficers.Remove(ucid);
        _lastRowTargets.Remove(ucid);
    }

    public IReadOnlyList<Player> GetNearbyNonOfficerTargets(Player officer, double rangeMeters)
    {
        return _playerManager.Players
            .Where(p => p.UCID != officer.UCID)
            .Where(p => _jobService.GetJob(p) != JobType.Police)
            .Select(p => (Player: p, Distance: Distance(officer, p)))
            .Where(x => x.Distance <= rangeMeters)
            .OrderBy(x => x.Distance)
            .Select(x => x.Player)
            .ToList();
    }

    private static double Distance(Player a, Player b)
    {
        var dx = a.Vehicle.X - b.Vehicle.X;
        var dy = a.Vehicle.Y - b.Vehicle.Y;

        return Math.Sqrt(dx * dx + dy * dy);
    }
}