using LfsCruise.Core.Vehicles.Market;
using LfsCruise.Core.Vehicles.Shop;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class MarketPage : MenuPage
{
    private const int PageSize = 5;

    // Layout konstantos - viskas 0-200 LFS UI vienetu skaleje (ne pikseliais!).
    // Palyginimui - senas Legacy stilius naudoja PanelWidth=65, Wide (MainMenuPage)
    // naudoja 90. Cia laikomes panasaus mastelio.
    private const byte PanelLeft = 45;
    private const byte HeaderTop = 46;
    private const byte RowHeight = 6;
    private const byte ContentWidth = 100;

    private const byte InfoWidth = 74;
    private const byte BuyButtonWidth = 24;
    private const byte BuyButtonGap = 2;
    private const byte BuyButtonLeft = PanelLeft + InfoWidth + BuyButtonGap;

    private const byte NavButtonWidth = 23;
    private const byte NavButtonGap = 2;

    private readonly IReadOnlyList<VehicleShopCategory> _categories;
    private readonly string _categoryId;
    private readonly int _page;
    private readonly IReadOnlyList<MarketListing> _listings;
    private readonly int _totalCount;
    private readonly int _playerBankBalance;

    public MarketPage(
        IReadOnlyList<VehicleShopCategory> categories,
        string categoryId,
        int page,
        IReadOnlyList<MarketListing> listings,
        int totalCount,
        int playerBankBalance)
    {
        _categories = categories;
        _categoryId = categoryId;
        _page = Math.Max(0, page);
        _listings = listings;
        _totalCount = totalCount;
        _playerBankBalance = playerBankBalance;
    }

    public override string Title => $"AUTO TURGUS ({_listings.Count}/{_totalCount})";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var buttons = new List<MenuButton>();

        var top = HeaderTop;

        // --- Kategoriju juosta (D | C | B | A | S) ---
        var visibleCategories = _categories.Take(ClickIds.Market.CategoryEnd - ClickIds.Market.CategoryStart + 1).ToList();
        var tabWidth = visibleCategories.Count > 0
            ? (byte)(ContentWidth / visibleCategories.Count)
            : ContentWidth;

        byte categoryClickId = ClickIds.Market.CategoryStart;
        byte tabLeft = PanelLeft;

        foreach (var category in visibleCategories)
        {
            var isSelected = category.Id.Equals(_categoryId, StringComparison.OrdinalIgnoreCase);
            var label = $"{category.Id} ({category.RequiredLicense:0}lic)";

            buttons.Add(new MenuButton
            {
                ClickId = categoryClickId++,
                Left = tabLeft,
                Top = top,
                Width = tabWidth,
                Height = RowHeight,
                Text = isSelected ? $"^2^3[{label}]" : $"^7{label}"
            });

            tabLeft = (byte)(tabLeft + tabWidth);
        }

        top += RowHeight;

        // --- Skelbimu eilutes (info + atskiras "Pirkti") ---

        if (_listings.Count == 0)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Market.ListingInfoStart,
                Left = PanelLeft,
                Top = top,
                Width = ContentWidth,
                Height = RowHeight,
                Text = "^7Sioje kategorijoje skelbimu nera",
                Enabled = false
            });

            top += RowHeight;
        }
        else
        {
            byte infoClickId = ClickIds.Market.ListingInfoStart;
            byte buyClickId = ClickIds.Market.ListingBuyStart;

            foreach (var listing in _listings)
            {
                const string kmText = "0.0km";

                buttons.Add(new MenuButton
                {
                    ClickId = infoClickId,
                    Left = PanelLeft,
                    Top = top,
                    Width = InfoWidth,
                    Height = RowHeight,
                    Enabled = false,
                    Text = $"^7{listing.DisplayName} ^8{kmText} ^2${listing.Price}"
                });

                buttons.Add(new MenuButton
                {
                    ClickId = buyClickId,
                    Left = BuyButtonLeft,
                    Top = top,
                    Width = BuyButtonWidth,
                    Height = RowHeight,
                    Text = "^2Pirkti"
                });

                infoClickId++;
                buyClickId++;
                top += RowHeight;
            }
        }

        // --- Info eilute (puslapis / mokejimas / bankas) ---
        var totalPages = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PageSize));
        var currentPage = Math.Clamp(_page, 0, totalPages - 1);

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Market.PageInfo,
            Left = PanelLeft,
            Top = top,
            Width = ContentWidth,
            Height = RowHeight,
            Enabled = false,
            Text = $"^7Psl {currentPage + 1}/{totalPages} ^8. ^7Mokejimas is banko ^8. ^7Bankas: ^2${_playerBankBalance}"
        });

        top += RowHeight;

        // --- Navigacijos eilute (Atgal | Psl navigacija | Uzdaryti) ---
        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Menu.Back,
            Left = PanelLeft,
            Top = top,
            Width = NavButtonWidth,
            Height = RowHeight,
            Text = "^7< Atgal"
        });

        if (currentPage > 0)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Market.PrevPage,
                Left = (byte)(PanelLeft + NavButtonWidth + NavButtonGap),
                Top = top,
                Width = NavButtonWidth,
                Height = RowHeight,
                Text = "^7< Psl"
            });
        }

        if (currentPage < totalPages - 1)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Market.NextPage,
                Left = (byte)(PanelLeft + (NavButtonWidth + NavButtonGap) * 2),
                Top = top,
                Width = NavButtonWidth,
                Height = RowHeight,
                Text = "^7Psl >"
            });
        }

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Menu.Close,
            Left = (byte)(PanelLeft + ContentWidth - NavButtonWidth),
            Top = top,
            Width = NavButtonWidth,
            Height = RowHeight,
            Text = "^1Uzdaryti"
        });

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

        if (clickId == ClickIds.Menu.Close)
            return manager.CloseAsync(player, cancellationToken);

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

        var listingIndex = clickId - ClickIds.Market.ListingBuyStart;

        if (listingIndex < 0 || listingIndex >= _listings.Count)
            return Task.CompletedTask;

        var listing = _listings[listingIndex];

        return manager.BuyMarketListingAsync(player, listing.Id, _categoryId, _page, cancellationToken);
    }
}