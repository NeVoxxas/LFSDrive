using LfsCruise.Core.Economy;
using LfsCruise.Core.UI;
using LfsCruise.Core.Vehicles;
using LfsCruise.Core.Vehicles.Shop;
using LfsCruise.Database;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class ShopVehiclesPage : MenuPage
{
    private readonly VehicleShopCategory _category;
    private readonly VehicleShopService _shopService;
    private readonly VehicleOwnershipService _ownershipService;
    private readonly EconomyService _economyService;
    private readonly DatabaseService _databaseService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    private readonly int _page;

    public ShopVehiclesPage(
        VehicleShopCategory category,
        VehicleShopService shopService,
        VehicleOwnershipService ownershipService,
        EconomyService economyService,
        DatabaseService databaseService,
        Func<byte, string, CancellationToken, Task> sendMessage,
        int page = 0)
    {
        _category = category;
        _shopService = shopService;
        _ownershipService = ownershipService;
        _economyService = economyService;
        _databaseService = databaseService;
        _sendMessage = sendMessage;
        _page = page;
    }

    public override string Title => _category.Name;

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        const int pageSize = 5;
        var vehicles = _category.Vehicles
            .Skip(_page * pageSize) .Take(pageSize) .ToList();

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

        if (_page > 0)
            "< Ankstesnis"
        if((_page + 1) * pageSize < _category.Vehicles.Count)
            "Kitas >"

        return buttons;
    }
    public override async Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Menu.Back)
        {
            await manager.OpenShopAsync(context.Player, cancellationToken);
            return;
        }

        var index = clickId - ClickIds.Shop.VehicleStart;

        if (index < 0 || index >= _category.Vehicles.Count)
            return;

        var vehicle = _category.Vehicles[index];
        var player = context.Player;

        if (await _ownershipService.OwnsVehicleAsync(player, vehicle.CarCode, cancellationToken))
        {
            await _sendMessage(player.UCID, $"^1Tu jau turi sia masina: ^7{vehicle.DisplayName}", cancellationToken);
            return;
        }

        if (!_economyService.RemoveMoney(player, vehicle.Price))
        {
            await _sendMessage(player.UCID, $"^1Nepakanka pinigu. Kaina: ^7{vehicle.Price}$", cancellationToken);
            return;
        }

        await _ownershipService.AddVehicleAsync(player, vehicle.CarCode, cancellationToken);
        await _databaseService.SavePlayerAsync(player, cancellationToken);

        await _sendMessage(player.UCID, $"^2Nusipirkai: ^7{vehicle.DisplayName} ^2uz ^7{vehicle.Price}$", cancellationToken);

        await manager.OpenVehicleCategoryAsync(player, _category, cancellationToken);
    }
}