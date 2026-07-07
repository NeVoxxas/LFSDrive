using LfsCruise.Core.Economy;
using LfsCruise.Core.Players;
using LfsCruise.Core.Vehicles.Demand;
using LfsCruise.Core.Vehicles.Market;
using LfsCruise.Core.Vehicles.Shop;
using LfsCruise.Database;

namespace LfsCruise.Core.Vehicles.Garage;

public sealed class GarageService
{
    public const double ServerSellRate = 0.5;   // 50% nuo salono kainos
    public const double MarketMaxRate = 2.0;    // max 2x nuo salono kainos

    private readonly VehicleOwnershipService _ownershipService;
    private readonly VehicleShopService _shopService;
    private readonly MarketStorage _marketStorage;
    private readonly EconomyService _economyService;
    private readonly DatabaseService _databaseService;
    private readonly VehicleDemandService _demandService;

    public GarageService(
        VehicleOwnershipService ownershipService,
        VehicleShopService shopService,
        MarketStorage marketStorage,
        EconomyService economyService,
        DatabaseService databaseService,
        VehicleDemandService demandService)
    {
        _ownershipService = ownershipService;
        _shopService = shopService;
        _marketStorage = marketStorage;
        _economyService = economyService;
        _databaseService = databaseService;
        _demandService = demandService;
    }

    // Grazina DABARTINE (paklausos pakoreguota) parduotuves kaina, ne baze.
    public int? GetShopPrice(string carCode)
    {
        var vehicle = _shopService.GetVehicleByCode(carCode);
        return vehicle is null ? null : _demandService.GetCurrentPrice(vehicle);
    }

    public static int GetServerSellPrice(int shopPrice) => (int)Math.Round(shopPrice * ServerSellRate);

    public static int GetMaxMarketPrice(int shopPrice) => (int)Math.Round(shopPrice * MarketMaxRate);

    public async Task<GarageResult> SellToServerAsync(
        Player player, string carCode, CancellationToken cancellationToken = default)
    {
        if (!await _ownershipService.OwnsVehicleAsync(player, carCode, cancellationToken))
            return GarageResult.Fail("Tu nesi sios transporto priemones savininkas.");

        if (await _marketStorage.IsCarListedAsync(player.Data.Id, carCode, cancellationToken))
            return GarageResult.Fail("Si transporto priemone iskelta i auto turga. Pirma atsiimk skelbima.");

        var shopPrice = GetShopPrice(carCode);

        if (shopPrice is null)
            return GarageResult.Fail("Si transporto priemone neturi nustatytos pardavimo kainos.");

        var sellPrice = GetServerSellPrice(shopPrice.Value);

        var removed = await _ownershipService.RemoveVehicleAsync(player, carCode, cancellationToken);

        if (!removed)
            return GarageResult.Fail("Nepavyko parduoti transporto priemones.");

        _economyService.AddMoney(player, sellPrice);
        await _databaseService.SavePlayerAsync(player, cancellationToken);

        // Vienu savininku maziau - iskart perskaiciuojam sitos masinos paklausa,
        // kad naujoji (zemesne) kaina matytusi be laukimo kito periodinio refresh.
        await _demandService.RefreshOneAsync(carCode, cancellationToken);

        return GarageResult.Ok($"Transporto priemone parduota serveriui uz {sellPrice}$.");
    }
}
