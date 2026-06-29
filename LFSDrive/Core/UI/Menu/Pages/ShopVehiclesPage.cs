using LfsCruise.Core.UI;
using LfsCruise.Core.Vehicles.Shop;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class ShopVehiclesPage : MenuPage
{
    private readonly VehicleShopCategory _category;
    private readonly VehicleShopService _shopService;


    public ShopVehiclesPage(
        VehicleShopCategory category,
        VehicleShopService shopService)
    {
        _category = category;
        _shopService = shopService;
    }

    public override string Title => _category.Name;

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

        byte clickId = ClickIds.Shop.VehicleStart;

        foreach (var vehicle in _category.Vehicles)
        {
            buttons.Add(new MenuButton
            {
                ClickId = clickId++,
                Text = $"^2{vehicle.DisplayName} ^7- ${vehicle.Price}"
            });
        }

        return buttons;
    }
    public override Task HandleClickAsync(MenuManager manager, MenuContext context, byte clickId, CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Menu.Back)
            return manager.OpenShopAsync(context.Player, cancellationToken);

        return Task.CompletedTask;
    }
}