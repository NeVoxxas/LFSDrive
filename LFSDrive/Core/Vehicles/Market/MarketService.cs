using LfsCruise.Core.Players;
using LfsCruise.Core.Vehicles.Shop;
using LfsCruise.Core.Vehicles.Garage;
using LfsCruise.Database;
using System.Numerics;

namespace LfsCruise.Core.Vehicles.Market;

public sealed class MarketService
{

    public const int PageSize = 5;

    private readonly MarketStorage _storage;
    private readonly VehicleOwnershipService _ownershipService;
    private readonly VehicleShopService _shopService;
    private readonly DatabaseService _databaseService;
    private readonly PlayerManager _playerManager;

    public MarketService(
        MarketStorage storage,
        VehicleOwnershipService ownershipService,
        VehicleShopService shopService,
        DatabaseService databaseService,
        PlayerManager playerManager)
    {
        _storage = storage;
        _ownershipService = ownershipService;
        _shopService = shopService;
        _databaseService = databaseService;
        _playerManager = playerManager;
    }

    public Task<(IReadOnlyList<MarketListing> Items, int TotalCount)> GetListingsAsync(
        string categoryId, int page, CancellationToken cancellationToken = default)
    {
        return _storage.GetPageAsync(categoryId, Math.Max(0, page), PageSize, cancellationToken);
    }

    public async Task<MarketResult> CreateListingAsync(Player player, int price, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(player.CarName))
            return MarketResult.Fail("Tu nesi automobilyje.");

        if (price <= 0)
            return MarketResult.Fail("Kaina turi buti teigiama.");

        if (!await _ownershipService.OwnsVehicleAsync(player, player.CarName, cancellationToken))
            return MarketResult.Fail("Tu nesi sio automobilio savininkas.");

        if (await _storage.IsCarListedAsync(player.Data.Id, player.CarName, cancellationToken))
            return MarketResult.Fail("Sis automobilis jau iskeltas i turgu.");

        var category = _shopService.GetCategoryForCarCode(player.CarName)
            ?? _shopService.GetCategories().OrderBy(c => c.RequiredLicense).FirstOrDefault();

        if (category is null)
            return MarketResult.Fail("Turgus dar nesukonfiguruotas.");

        var displayName = _shopService.GetVehicleByCode(player.CarName)?.DisplayName ?? player.CarName;

        await _storage.AddAsync(new MarketListing
        {
            SellerPlayerId = player.Data.Id,
            SellerUsername = player.Username,
            CarCode = player.CarName,
            DisplayName = displayName,
            CategoryId = category.Id,
            Price = price
        }, cancellationToken);

        return MarketResult.Ok($"Automobilis {displayName} iskeltas i turgu uz {price}$.");
    }

    public async Task<MarketResult> BuyAsync(Player buyer, int listingId, CancellationToken cancellationToken)
    {
        var listing = await _storage.GetByIdAsync(listingId, cancellationToken);

        if (listing is null)
            return MarketResult.Fail("Sis skelbimas jau nebegaliojantis.");

        if (listing.SellerPlayerId == buyer.Data.Id)
            return MarketResult.Fail("Negali pirkti savo paties automobilio.");

        if (buyer.Data.Bank < listing.Price)
            return MarketResult.Fail("Nepakanka pinigu banke.");

        if (await _ownershipService.OwnsVehicleAsync(buyer, listing.CarCode, cancellationToken))
            return MarketResult.Fail("Tu jau turi sia masina.");

        // Ar dar galioja?

        var claimed = await _storage.RemoveAsync(listingId, cancellationToken);

        if (!claimed)
            return MarketResult.Fail("Sis skelbimas jau nebegaliojantis.");

        buyer.Data.Bank -= listing.Price;
        await _databaseService.SavePlayerAsync(buyer, cancellationToken);

        await CreditSellerAsync(listing.SellerPlayerId, listing.Price, cancellationToken);

        // Ownership + doc transfer

        await _ownershipService.TransferVehicleAsync(
            listing.SellerPlayerId, buyer.Data.Id, listing.CarCode, cancellationToken);

        return MarketResult.Ok($"Nupirkai {listing.DisplayName} uz {listing.Price}$.");
    }

    private async Task CreditSellerAsync(int sellerPlayerId, int amount, CancellationToken cancellationToken)
    {
        var sellerPlayer = _playerManager.Players.FirstOrDefault(p => p.Data.Id == sellerPlayerId);

        if (sellerPlayer is not null)
        {
            sellerPlayer.Data.Bank += amount;
            await _databaseService.SavePlayerAsync(sellerPlayer, cancellationToken);
            return;
        }

        await _databaseService.AddBankBalanceAsync(sellerPlayerId, amount, cancellationToken);
    }

    public async Task<MarketResult> CreateListingForCarAsync(
    Player player, string carCode, int price, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(carCode))
            return MarketResult.Fail("Neteisinga transporto priemone.");

        if (price <= 0)
            return MarketResult.Fail("Kaina turi buti teigiama.");

        if (!await _ownershipService.OwnsVehicleAsync(player, carCode, cancellationToken))
            return MarketResult.Fail("Tu nesi sios transporto priemones savininkas.");

        if (await _storage.IsCarListedAsync(player.Data.Id, carCode, cancellationToken))
            return MarketResult.Fail("Sis automobilis jau iskeltas i turga.");

        var shopVehicle = _shopService.GetVehicleByCode(carCode);

        if (shopVehicle is null)
            return MarketResult.Fail("Si transporto priemone neturi nustatytos bazines kainos.");

        var maxPrice = GarageService.GetMaxMarketPrice(shopVehicle.Price);

        if (price > maxPrice)
            return MarketResult.Fail($"Maksimali kaina siai masinai: {maxPrice}$.");

        var category = _shopService.GetCategoryForCarCode(carCode)
            ?? _shopService.GetCategories().OrderBy(c => c.RequiredLicense).FirstOrDefault();

        if (category is null)
            return MarketResult.Fail("Turgus dar nesukonfiguruotas.");

        await _storage.AddAsync(new MarketListing
        {
            SellerPlayerId = player.Data.Id,
            SellerUsername = player.Username,
            CarCode = carCode,
            DisplayName = shopVehicle.DisplayName,
            CategoryId = category.Id,
            Price = price
        }, cancellationToken);

        return MarketResult.Ok($"Automobilis {shopVehicle.DisplayName} iskeltas i turga uz {price}$.");
    }
}

