using LfsCruise.Core.Economy;
using LfsCruise.Core.UI;
using LfsCruise.Core.Vehicles;
using LfsCruise.Core.Vehicles.Shop;
using LfsCruise.Database;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class ShopVehiclesPage : MenuPage
{
    private const int PageSize = 5;

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
        _page = Math.Max(0, page);
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

        var totalVehicles = _category.Vehicles.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalVehicles / (double)PageSize));
        var currentPage = Math.Clamp(_page, 0, totalPages - 1);

        var pageVehicles = _category.Vehicles
            .Skip(currentPage * PageSize)
            .Take(PageSize)
            .ToList();

        byte clickId = ClickIds.Shop.VehicleStart;

        foreach (var vehicle in pageVehicles)
        {
            buttons.Add(new MenuButton
            {
                ClickId = clickId++,
                Text = $"^2{vehicle.DisplayName} ^7- ${vehicle.Price}"
            });
        }

        if (currentPage > 0)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Shop.PreviousPage,
                Text = "^7< Ankstesnis"
            });
        }

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Shop.PageInfo,
            Text = $"^7Puslapis {currentPage + 1}/{totalPages}"
        });

        if (currentPage < totalPages - 1)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Shop.NextPage,
                Text = "^7Kitas >"
            });
        }

        return buttons;
    }

    public override async Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        var player = context.Player;

        if (clickId == ClickIds.Menu.Back)
        {
            await manager.OpenShopAsync(player, cancellationToken);
            return;
        }

        if (clickId == ClickIds.Shop.PreviousPage)
        {
            await manager.OpenVehicleCategoryAsync(player, _category, _page - 1, cancellationToken);
            return;
        }

        if (clickId == ClickIds.Shop.NextPage)
        {
            await manager.OpenVehicleCategoryAsync(player, _category, _page + 1, cancellationToken);
            return;
        }

        if (clickId == ClickIds.Shop.PageInfo)
            return;

        var indexOnPage = clickId - ClickIds.Shop.VehicleStart;

        if (indexOnPage < 0 || indexOnPage >= PageSize)
            return;

        var vehicleIndex = (_page * PageSize) + indexOnPage;

        if (vehicleIndex < 0 || vehicleIndex >= _category.Vehicles.Count)
            return;

        var vehicle = _category.Vehicles[vehicleIndex];

        if (player.License < _category.RequiredLicense)
        {
            await _sendMessage(
                player.UCID,
                $"^1Nepakanka licenzijos. Reikia: ^7{_category.RequiredLicense:0.0} ^1Tavo: ^7{player.License:0.0}",
                cancellationToken);

            return;
        }

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

        await manager.OpenVehicleCategoryAsync(player, _category, _page, cancellationToken);
    }
}