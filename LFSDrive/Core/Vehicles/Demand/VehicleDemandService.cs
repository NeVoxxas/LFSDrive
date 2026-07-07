using LfsCruise.Core.Vehicles.Shop;

namespace LfsCruise.Core.Vehicles.Demand;

// Dinamine parduotuves kaina pagal TAI, kiek zaideju SIUO METU turi tam tikra masina (owned_vehicles lentele)
//
// Kuo daugiau savininku - tuo brangesne masina.
// Kuo maziau - tuo pigesne.
//
// Savininku skaiciai laikomi cache, DB uzklausa kiekvienam
// parduotuves atnaujinimui privestu prie perfomance loss.
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

    // Perkraunam configa be restarto
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

    // Atnaujina kiekviena savininka is DB fone.
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var counts = await _ownershipService.GetOwnerCountsAsync(cancellationToken);
        _ownerCounts = counts;
    }

    // Perkraunam vienos masinos savininku skaiciu. ( po pirkimo/pardavimo )
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
