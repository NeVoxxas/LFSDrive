using System.Text.RegularExpressions;
using LfsCruise.Core.Economy;
using LfsCruise.Core.Players;
using LfsCruise.Database;

namespace LfsCruise.Core.Vehicles.Regitra;

public sealed class RegitraService
{
    private static readonly Regex PlateRegex = new(@"^[A-Z0-9]{3,8}$", RegexOptions.Compiled);

    private readonly RegitraStorage _storage;
    private readonly RegitraConfigStorage _configStorage;
    private readonly EconomyService _economyService;
    private readonly VehicleOwnershipService _ownershipService;
    private readonly DatabaseService _databaseService;

    private RegitraConfig _config;

    public RegitraService(
        RegitraStorage storage,
        RegitraConfigStorage configStorage,
        EconomyService economyService,
        VehicleOwnershipService ownershipService,
        DatabaseService databaseService)
    {
        _storage = storage;
        _configStorage = configStorage;
        _economyService = economyService;
        _ownershipService = ownershipService;
        _databaseService = databaseService;

        _config = _configStorage.Load();
    }

    public RegitraConfig Config => _config;

    public void ReloadConfig()
    {
        _config = _configStorage.Load();
    }

    public Task<VehicleDocuments?> GetDocumentsAsync(
        Player player, string carCode, CancellationToken cancellationToken = default)
    {
        return _storage.GetDocumentsAsync(player, carCode, cancellationToken);
    }

    public async Task<RegitraResult> BuyPlateAsync(
        Player player, string carCode, string rawPlateNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(carCode))
            return RegitraResult.Fail("Tu nesi automobilyje.");

        if (!await _ownershipService.OwnsVehicleAsync(player, carCode, cancellationToken))
            return RegitraResult.Fail("Tu nesi sio automobilio savininkas.");

        if (!TryNormalizePlate(rawPlateNumber, out var plateNumber))
            return RegitraResult.Fail("Neteisingas numerio formatas. (3-8 simboliai, A-Z 0-9)");

        var existing = await _storage.GetDocumentsAsync(player, carCode, cancellationToken);

        if (existing?.HasPlate == true)
            return RegitraResult.Fail("Sis automobilis jau turi numerius. Naudok numeriu keitima.");

        if (await _storage.IsPlateTakenAsync(plateNumber, cancellationToken))
            return RegitraResult.Fail($"Numeriai {plateNumber} jau uzimti.");

        if (!_economyService.RemoveMoney(player, _config.PlatePrice))
            return RegitraResult.Fail($"Nepakanka pinigu. Kaina: {_config.PlatePrice}$");

        await _storage.SetPlateAsync(player, carCode, plateNumber, cancellationToken);
        await _databaseService.SavePlayerAsync(player, cancellationToken);

        return RegitraResult.Ok($"Numeriai {plateNumber} isigyti uz {_config.PlatePrice}$.");
    }

    public async Task<RegitraResult> ChangePlateAsync(
        Player player, string carCode, string rawPlateNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(carCode))
            return RegitraResult.Fail("Tu nesi automobilyje.");

        if (!await _ownershipService.OwnsVehicleAsync(player, carCode, cancellationToken))
            return RegitraResult.Fail("Tu nesi sio automobilio savininkas.");

        if (!TryNormalizePlate(rawPlateNumber, out var plateNumber))
            return RegitraResult.Fail("Neteisingas numerio formatas. (3-8 simboliai, A-Z 0-9)");

        var existing = await _storage.GetDocumentsAsync(player, carCode, cancellationToken);

        if (existing?.HasPlate != true)
            return RegitraResult.Fail("Sis automobilis dar neturi numeriu. Pirma juos isigyk.");

        if (string.Equals(existing.PlateNumber, plateNumber, StringComparison.OrdinalIgnoreCase))
            return RegitraResult.Fail("Tokie numeriai jau priskirti siam automobiliui.");

        if (await _storage.IsPlateTakenAsync(plateNumber, cancellationToken))
            return RegitraResult.Fail($"Numeriai {plateNumber} jau uzimti.");

        if (!_economyService.RemoveMoney(player, _config.PlateChangePrice))
            return RegitraResult.Fail($"Nepakanka pinigu. Kaina: {_config.PlateChangePrice}$");

        await _storage.SetPlateAsync(player, carCode, plateNumber, cancellationToken);
        await _databaseService.SavePlayerAsync(player, cancellationToken);

        return RegitraResult.Ok($"Numeriai pakeisti i {plateNumber} uz {_config.PlateChangePrice}$.");
    }

    public async Task<RegitraResult> BuyInsuranceAsync(
        Player player, string carCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(carCode))
            return RegitraResult.Fail("Tu nesi automobilyje.");

        if (!await _ownershipService.OwnsVehicleAsync(player, carCode, cancellationToken))
            return RegitraResult.Fail("Tu nesi sio automobilio savininkas.");

        if (!_economyService.RemoveMoney(player, _config.InsurancePrice))
            return RegitraResult.Fail($"Nepakanka pinigu. Kaina: {_config.InsurancePrice}$");

        var existing = await _storage.GetDocumentsAsync(player, carCode, cancellationToken);

        var baseDate = existing?.InsuranceExpiresAt is { } expiresAt && expiresAt > DateTime.UtcNow
            ? expiresAt
            : DateTime.UtcNow;

        var newExpiresAt = baseDate.AddDays(_config.InsuranceDurationDays);

        await _storage.SetInsuranceAsync(player, carCode, newExpiresAt, cancellationToken);
        await _databaseService.SavePlayerAsync(player, cancellationToken);

        return RegitraResult.Ok($"Draudimas galioja iki {newExpiresAt:yyyy-MM-dd}.");
    }

    public async Task<RegitraResult> BuyInspectionAsync(
        Player player, string carCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(carCode))
            return RegitraResult.Fail("Tu nesi automobilyje.");

        if (!await _ownershipService.OwnsVehicleAsync(player, carCode, cancellationToken))
            return RegitraResult.Fail("Tu nesi sio automobilio savininkas.");

        if (!_economyService.RemoveMoney(player, _config.InspectionPrice))
            return RegitraResult.Fail($"Nepakanka pinigu. Kaina: {_config.InspectionPrice}$");

        var existing = await _storage.GetDocumentsAsync(player, carCode, cancellationToken);

        var baseDate = existing?.InspectionExpiresAt is { } expiresAt && expiresAt > DateTime.UtcNow
            ? expiresAt
            : DateTime.UtcNow;

        var newExpiresAt = baseDate.AddMonths(_config.InspectionDurationMonths);

        await _storage.SetInspectionAsync(player, carCode, newExpiresAt, cancellationToken);
        await _databaseService.SavePlayerAsync(player, cancellationToken);

        return RegitraResult.Ok($"Technine apziura galioja iki {newExpiresAt:yyyy-MM-dd}.");
    }

    private static bool TryNormalizePlate(string raw, out string plateNumber)
    {
        plateNumber = (raw ?? string.Empty).Trim().ToUpperInvariant();
        return PlateRegex.IsMatch(plateNumber);
    }
}