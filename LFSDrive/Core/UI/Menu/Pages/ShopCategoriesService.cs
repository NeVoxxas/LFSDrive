using LfsCruise.Core.UI;
using LfsCruise.Core.Vehicles.Shop;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class ShopCategoriesPage : MenuPage
{
    private readonly VehicleShopService _shopService;

    public ShopCategoriesPage(VehicleShopService shopService)
    {
        _shopService = shopService;
    }

    public override string Title => "Parduotuve";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var buttons = new List<MenuButton>
        {
            new()
            {
                ClickId = ClickIds.Menu.Back,
                Text = "^7< Atgal"
            }
        };

        byte clickId = ClickIds.Shop.CategoryStart;

        foreach (var category in _shopService.GetCategories())
        {
            buttons.Add(new MenuButton
            {
                ClickId = clickId++,
                Text = $"^2{category.Name} ^7({category.RequiredLicense:F0} Lic)"
            });
        }

        return buttons;
    }

    public override Task HandleClickAsync(
    MenuManager manager,
    MenuContext context,
    byte clickId,
    CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Menu.Back)
            return manager.OpenMainMenuAsync(context.Player, cancellationToken);

        var index = clickId - ClickIds.Shop.CategoryStart;

        var categories = _shopService.GetCategories();

        if (index >= 0 && index < categories.Count)
        {
            return manager.OpenVehicleCategoryAsync(
                context.Player,
                categories[index],
                cancellationToken);
        }

        return Task.CompletedTask;
    }
}