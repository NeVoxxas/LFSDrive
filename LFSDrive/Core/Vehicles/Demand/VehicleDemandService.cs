using LfsCruise.Core.Vehicles.Shop;

namespace LfsCruise.Core.Vehicles.Demand;
// Kiekviena masina turi paklausos multip. (bazinis = 1.0x):
//   - pirkimas kelia multip.   (+PurchaseBumpPerSale)
//   - pardavimas serveriui zemina ji   (-SaleBackPenalty)
//   - laikui begant daugiklis savaime gryzta atgal iki 1.0x
//     su exponentiniu pusejo periodu (HalfLifeHours)
//
// bazine kaina (VehicleShopItem.Price iš vehicle_shop.json) NIEKADA nera
// perrasoma - ji visada yra "inkaras", nuo kurio skaiciuojama dabartine kaina.
public sealed class VehicleDemandService
{
    private const double PurchaseBumpPerSale = 0.08; // +8% už kiekvieną pirkimą
    private const double SaleBackPenalty = 0.05;      // -5% už pardavimą atgal serveriui
    private const double MinMultiplier = 0.7;
    private const double MaxMultiplier = 2.0;
    private const double HalfLifeHours = 6.0;

    private readonly VehicleDemandStorage _storage;
    private readonly VehicleDemandConfig _config;

    public VehicleDemandService(VehicleDemandStorage storage)
    {
        _storage = storage;
        _config = storage.Load();
    }

    public int GetCurrentPrice(VehicleShopItem vehicle)
    {
        var multiplier = GetCurrentMultiplier(vehicle.CarCode);
        return (int)Math.Max(1, Math.Round(vehicle.Price * multiplier));
    }

    public double GetCurrentMultiplier(string carCode)
    {
        return _config.States.TryGetValue(carCode, out var state)
            ? Decay(state)
            : 1.0;
    }

    public void RegisterPurchase(VehicleShopItem vehicle)
    {
        Adjust(vehicle.CarCode, PurchaseBumpPerSale);
    }

    public void RegisterSaleToServer(string carCode)
    {
        Adjust(carCode, -SaleBackPenalty);
    }

    private void Adjust(string carCode, double delta)
    {
        if (!_config.States.TryGetValue(carCode, out var state))
        {
            state = new VehicleDemandState();
            _config.States[carCode] = state;
        }

        var decayedMultiplier = Decay(state);
        var newMultiplier = Math.Clamp(decayedMultiplier + delta, MinMultiplier, MaxMultiplier);

        state.Multiplier = newMultiplier;
        state.LastUpdatedAt = DateTime.UtcNow;

        _storage.Save(_config);
    }

    private static double Decay(VehicleDemandState state)
    {
        var elapsedHours = (DateTime.UtcNow - state.LastUpdatedAt).TotalHours;

        if (elapsedHours <= 0)
            return state.Multiplier;

        var decayFactor = Math.Pow(0.5, elapsedHours / HalfLifeHours);
        var excess = state.Multiplier - 1.0;
        var decayedExcess = excess * decayFactor;

        return Math.Clamp(1.0 + decayedExcess, MinMultiplier, MaxMultiplier);
    }
}
