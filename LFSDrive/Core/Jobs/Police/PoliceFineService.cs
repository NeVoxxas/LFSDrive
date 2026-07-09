using LfsCruise.Core.Economy;
using LfsCruise.Core.Players;
using LfsCruise.Database;

namespace LfsCruise.Core.Jobs.Police;

public sealed class PoliceFineService
{
    private const double MaxStopSpeedKmh = 1.0;
    private static readonly TimeSpan FineCooldown = TimeSpan.FromSeconds(30);

    private readonly PoliceViolationStorage _violationStorage;
    private readonly EconomyService _economyService;
    private readonly DatabaseService _databaseService;

    private readonly Dictionary<(byte Officer, byte Target), DateTime> _lastFineAt = new();

    public PoliceFineService(
        PoliceViolationStorage violationStorage,
        EconomyService economyService,
        DatabaseService databaseService)
    {
        _violationStorage = violationStorage;
        _economyService = economyService;
        _databaseService = databaseService;
    }

    public PoliceViolationConfig LoadViolations() => _violationStorage.Load();

    public async Task<PoliceFineResult> ApplyFinesAsync(
        Player officer, Player target, IReadOnlyCollection<string> violationIds, CancellationToken cancellationToken)
    {
        if (officer.Vehicle.Speed > MaxStopSpeedKmh || target.Vehicle.Speed > MaxStopSpeedKmh)
            return PoliceFineResult.Fail("Abu turite buti sustoje.");

        var key = (officer.UCID, target.UCID);

        if (_lastFineAt.TryGetValue(key, out var lastAt) && DateTime.UtcNow - lastAt < FineCooldown)
            return PoliceFineResult.Fail("Palauk truputi pries baudziant si zaideja is naujo.");

        var config = _violationStorage.Load();
        var selected = config.Violations.Where(v => violationIds.Contains(v.Id)).ToList();

        if (selected.Count == 0)
            return PoliceFineResult.Fail("Nepasirinktas nei vienas pazeidimas.");

        var total = selected.Sum(v => v.Fine);

        if (!_economyService.RemoveMoney(target, total))
            return PoliceFineResult.Fail($"Zaidejas neturi uztektinai pinigu ({total}$).");

        var officerShare = (int)Math.Round(total * config.OfficerSharePercent);

        if (officerShare > 0)
            _economyService.AddMoney(officer, officerShare);

        _lastFineAt[key] = DateTime.UtcNow;

        await _databaseService.SavePlayerAsync(target, cancellationToken);
        await _databaseService.SavePlayerAsync(officer, cancellationToken);

        var violationNames = string.Join(", ", selected.Select(v => v.Name));

        return PoliceFineResult.Ok($"Israsyta bauda {target.Username}: {total}$ ({violationNames}). Tau: {officerShare}$.");
    }
}