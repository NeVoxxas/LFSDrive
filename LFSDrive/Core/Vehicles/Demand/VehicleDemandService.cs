using LfsCruise.Core.Vehicles.Shop;

namespace LfsCruise.Core.Vehicles.Demand;

// Dinamine parduotuves kaina pagal TAI, kiek zaideju SIUO METU turi masina
// (owned_vehicles lentele), ne pagal irasytus pirkimo/pardavimo ivykius.
//
// Kuo daugiau savininku - tuo brangesne masina (populiarumo/hype mokestis).
// Kuo maziau - tuo pigesne (paskatinti pirkti nepopuliaria masina).
//
// Savininku skaiciai laikomi atmintyje (cache), nes DB uzklausa kiekvienam
// parduotuves puslapio atvaizdavimui butu per brangu. Cache atnaujinamas:
//   - periodiskai fone (VehicleDemandRefreshLoop, kas kelias minutes)
//   - iskart po konkretaus pirkimo/pardavimo (RefreshOneAsync), kad UI
//     atsinaujintu greitai, ne palaukus kito periodinio ciklo
public sealed class VehicleDemandService
{
    private readonly VehicleOwnershipService _ownershipService;
    private readonly VehicleDemandConfigStorage _configStorage;

    private List<VehicleDemandTier> _tiers;
    private Dictionary<string, int> _ownerCounts = new(StringComparer.OrdinalIgnoreCase);

    public VehicleDemandService(
        VehicleOwnershipService ownershipService,
        VehicleDemandConfigStorage configStorage)
    {
        _ownershipService = ownershipService;
        _configStorage = configStorage;
        _tiers = LoadSortedTiers();
    }

    // Leidzia perkrauti pakopu konfiguracija be serverio restart (pvz. is admin komandos).
    public void ReloadTiersConfig()
    {
        _tiers = LoadSortedTiers();
    }

    private List<VehicleDemandTier> LoadSortedTiers()
    {
        return _configStorage.Load()
            .Tiers
            .OrderBy(t => t.MinOwners)
            .ToList();
    }

    // Perkrauna VISU masinu savininku skaicius is DB. Kviesti periodiskai fone.
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var counts = await _ownershipService.GetOwnerCountsAsync(cancellationToken);
        _ownerCounts = counts;
    }

    // Perkrauna VIENOS masinos savininku skaiciu - naudinga iskart po pirkimo/pardavimo,
    // kad kaina UI atsinaujintu be laukimo kito periodinio refresh.
    public async Task RefreshOneAsync(string carCode, CancellationToken cancellationToken = default)
    {
        var count = await _ownershipService.GetOwnerCountAsync(carCode, cancellationToken);
        _ownerCounts[carCode] = count;
    }

    public int GetOwnerCount(string carCode)
    {
        return _ownerCounts.TryGetValue(carCode, out var count) ? count : 0;
    }

    public double GetCurrentMultiplier(string carCode)
    {
        var ownerCount = GetOwnerCount(carCode);
        var multiplier = 1.0;

        foreach (var tier in _tiers)
        {
            if (ownerCount >= tier.MinOwners)
                multiplier = tier.Multiplier;
        }

        return multiplier;
    }

    public int GetCurrentPrice(VehicleShopItem vehicle)
    {
        var multiplier = GetCurrentMultiplier(vehicle.CarCode);
        return (int)Math.Max(1, Math.Round(vehicle.Price * multiplier));
    }
}
