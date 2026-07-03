using LfsCruise.Core.Vehicles.Market;
using LfsCruise.Core.Vehicles.Shop;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class MarketPage : MenuPage
{
    private const int PageSize = 5;

    private readonly IReadOnlyList<VehicleShopCategory> _categories;
    private readonly string _categoryId;
    private readonly int _page;
    private readonly IReadOnlyList<MarketListing> _listings;
    private readonly int _totalCount;

    public MarketPage(
        IReadOnlyList<VehicleShopCategory> categories,
        string categoryId,
        int page,
        IReadOnlyList<MarketListing> listings,
        int totalCount)
    {
        _categories = categories;
        _categoryId = categoryId;
        _page = Math.Max(0, page);
        _listings = listings;
        _totalCount = totalCount;
    }

    public override string Title => "Auto turgus";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var buttons = new List<MenuButton>
        {
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" }
        };

        byte categoryClickId = ClickIds.Market.CategoryStart;

        foreach (var category in _categories)
        {
            if (categoryClickId > ClickIds.Market.CategoryEnd)
                break;

            var isSelected = category.Id.Equals(_categoryId, StringComparison.OrdinalIgnoreCase);

            buttons.Add(new MenuButton
            {
                ClickId = categoryClickId++,
                Text = isSelected ? $"^2[{category.Name}]" : $"^7{category.Name}"
            });
        }

        var totalPages = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PageSize));
        var currentPage = Math.Clamp(_page, 0, totalPages - 1);

        byte listingClickId = ClickIds.Market.ListingStart;

        foreach (var listing in _listings)
        {
            buttons.Add(new MenuButton
            {
                ClickId = listingClickId++,
                Text = $"^2{listing.DisplayName} ^7- ${listing.Price} ^3[Pirkti]"
            });
        }

        if (_listings.Count == 0)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Market.ListingStart,
                Text = "^7Sioje kategorijoje skelbimu nera",
                Enabled = false
            });
        }

        if (currentPage > 0)
        {
            buttons.Add(new MenuButton { ClickId = ClickIds.Market.PrevPage, Text = "^7< Ankstesnis" });
        }

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Market.PageInfo,
            Text = $"^7Puslapis {currentPage + 1}/{totalPages}",
            Enabled = false
        });

        if (currentPage < totalPages - 1)
        {
            buttons.Add(new MenuButton { ClickId = ClickIds.Market.NextPage, Text = "^7Kitas >" });
        }

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        var player = context.Player;

        if (clickId == ClickIds.Menu.Back)
            return manager.OpenMainMenuAsync(player, cancellationToken);

        if (clickId >= ClickIds.Market.CategoryStart && clickId <= ClickIds.Market.CategoryEnd)
        {
            var index = clickId - ClickIds.Market.CategoryStart;

            if (index >= 0 && index < _categories.Count)
                return manager.OpenMarketAsync(player, _categories[index].Id, 0, cancellationToken);

            return Task.CompletedTask;
        }

        if (clickId == ClickIds.Market.PrevPage)
            return manager.OpenMarketAsync(player, _categoryId, _page - 1, cancellationToken);

        if (clickId == ClickIds.Market.NextPage)
            return manager.OpenMarketAsync(player, _categoryId, _page + 1, cancellationToken);

        if (clickId == ClickIds.Market.PageInfo)
            return Task.CompletedTask;

        var listingIndex = clickId - ClickIds.Market.ListingStart;

        if (listingIndex < 0 || listingIndex >= _listings.Count)
            return Task.CompletedTask;

        var listing = _listings[listingIndex];

        return manager.BuyMarketListingAsync(player, listing.Id, _categoryId, _page, cancellationToken);
    }
}