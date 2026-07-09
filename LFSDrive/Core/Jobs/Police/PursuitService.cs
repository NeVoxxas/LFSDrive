using LfsCruise.Core.Players;

namespace LfsCruise.Core.Jobs.Police;

public sealed class PursuitState
{
    public required HashSet<byte> OfficerUcids { get; init; }
    public required string PrimaryOfficerName { get; set; }
    public int WantedLevel { get; set; } = 1;
    public DateTime LastLevelIncreaseAt { get; set; } = DateTime.UtcNow;

    public DateTime? SafeSince { get; set; }
}
public sealed class PursuitService
{
    public const double EscapeDistanceMeters = 150.0;
    public static readonly TimeSpan EscapeDuration = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan LevelIncreaseInterval = TimeSpan.FromSeconds(45);
    public const int MaxWantedLevel = 5;

    private readonly PlayerManager _playerManager;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    private readonly Dictionary<byte, PursuitState> _pursuits = new();

    public PursuitService(PlayerManager playerManager, Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _playerManager = playerManager;
        _sendMessage = sendMessage;
    }

    public bool IsWanted(byte targetUcid) => _pursuits.ContainsKey(targetUcid);

    public (int Level, string OfficerName)? GetPursuitInfo(byte targetUcid)
    {
        return _pursuits.TryGetValue(targetUcid, out var state)
            ? (state.WantedLevel, state.PrimaryOfficerName)
            : null;
    }

    public async Task StartPursuitAsync(Player officer, Player target, CancellationToken cancellationToken)
    {
        if (_pursuits.TryGetValue(target.UCID, out var existing))
        {
            existing.OfficerUcids.Add(officer.UCID);
            existing.SafeSince = null;

            await _sendMessage(
                officer.UCID,
                $"^3Prisijungei prie {target.Username} gaudymo (lygis {existing.WantedLevel}).",
                cancellationToken);

            return;
        }

        var state = new PursuitState
        {
            OfficerUcids = new HashSet<byte> { officer.UCID },
            PrimaryOfficerName = officer.Username,
            WantedLevel = 1,
            LastLevelIncreaseAt = DateTime.UtcNow,
            SafeSince = null
        };

        _pursuits[target.UCID] = state;

        await _sendMessage(officer.UCID, $"^2Pradejai gaudyti: ^7{target.Username}", cancellationToken);
        await _sendMessage(target.UCID, $"^1Tave pradejo gaudyti pareigunas ^7{officer.Username}^1!", cancellationToken);
    }

    public void RemoveOfficer(byte officerUcid)
    {
        foreach (var state in _pursuits.Values)
        {
            state.OfficerUcids.Remove(officerUcid);
        }
    }

    public async Task OnPlayerMoveAsync(Player player, CancellationToken cancellationToken)
    {
        if (!_pursuits.TryGetValue(player.UCID, out var state))
            return;

        state.OfficerUcids.RemoveWhere(ucid => _playerManager.Get(ucid) is null);

        if (state.OfficerUcids.Count == 0)
        {
            _pursuits.Remove(player.UCID);
            await _sendMessage(player.UCID, "^2Gaudymas baigtas.", cancellationToken);
            return;
        }

        var minDistance = double.MaxValue;

        foreach (var officerUcid in state.OfficerUcids)
        {
            var officer = _playerManager.Get(officerUcid);
            if (officer is null)
                continue;

            var dx = player.Vehicle.X - officer.Vehicle.X;
            var dy = player.Vehicle.Y - officer.Vehicle.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < minDistance)
                minDistance = distance;
        }

        var now = DateTime.UtcNow;

        if (minDistance <= EscapeDistanceMeters)
        {
            state.SafeSince = null;
        }
        else
        {
            state.SafeSince ??= now;

            if (now - state.SafeSince.Value >= EscapeDuration)
            {
                _pursuits.Remove(player.UCID);
                await _sendMessage(player.UCID, "^2Pabegai nuo policijos!", cancellationToken);
                return;
            }
        }

        if (state.WantedLevel < MaxWantedLevel && now - state.LastLevelIncreaseAt >= LevelIncreaseInterval)
        {
            state.WantedLevel++;
            state.LastLevelIncreaseAt = now;

            await _sendMessage(player.UCID, $"^1Gaudomumo lygis pakilo: ^7{state.WantedLevel}/{MaxWantedLevel}", cancellationToken);
        }
    }

    // PD UI - Gaudomi zaidejai. Grazina sarasa su (TargetName, Level, OfficerNames).
    public IReadOnlyList<(string TargetName, int Level, string OfficerNames)> GetActivePursuitSummaries()
    {
        var result = new List<(string, int, string)>();

        foreach (var (targetUcid, state) in _pursuits)
        {
            var target = _playerManager.Get(targetUcid);

            if (target is null)
                continue;

            var officerNames = state.OfficerUcids
                .Select(ucid => _playerManager.Get(ucid)?.Username)
                .Where(name => name is not null)
                .ToList();

            result.Add((target.Username, state.WantedLevel, string.Join(", ", officerNames)));
        }

        return result;
    }
}