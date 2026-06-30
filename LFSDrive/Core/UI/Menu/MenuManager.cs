using LfsCruise.Core.Economy;
using LfsCruise.Core.Players;
using LfsCruise.Core.UI.Menu.Pages;
using LfsCruise.Core.Vehicles;
using LfsCruise.Core.Vehicles.Shop;
using LfsCruise.Database;

namespace LfsCruise.Core.UI.Menu;

public sealed class MenuManager
{
    private readonly MenuRenderer _renderer;
    private readonly Dictionary<byte, MenuPage> _currentPages = new();

    private readonly MainMenuPage _mainMenuPage = new();
    private readonly VehicleShopService _vehicleShopService;
    private readonly ShopCategoriesPage _shopPage;
    private readonly VehicleOwnershipService _ownershipService;
    private readonly EconomyService _economyService;
    private readonly DatabaseService _databaseService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public MenuManager(
        MenuRenderer renderer,
        VehicleShopService vehicleShopService,
        VehicleOwnershipService ownershipService,
        EconomyService economyService,
        DatabaseService databaseService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _renderer = renderer;
        _vehicleShopService = vehicleShopService;
        _ownershipService = ownershipService;
        _economyService = economyService;
        _databaseService = databaseService;
        _sendMessage = sendMessage;

        _shopPage = new ShopCategoriesPage(vehicleShopService);
    }

    public async Task OpenMainMenuAsync(Player player, CancellationToken cancellationToken)
    {
        await OpenPageAsync(player, _mainMenuPage, cancellationToken);
    }

    public async Task OpenPageAsync(Player player, MenuPage page, CancellationToken cancellationToken)
    {
        await _renderer.CloseAsync(player, cancellationToken);

        _currentPages[player.UCID] = page;

        await _renderer.RenderAsync(player, page, cancellationToken);
    }

    public async Task HandleClickAsync(Player player, byte clickId, CancellationToken cancellationToken)
    {
        if (!_currentPages.TryGetValue(player.UCID, out var page))
            return;

        var context = new MenuContext
        {
            Player = player
        };



        await page.HandleClickAsync(this, context, clickId, cancellationToken);
    }

    public async Task CloseAsync(Player player, CancellationToken cancellationToken)
    {
        _currentPages.Remove(player.UCID);
        await _renderer.CloseAsync(player, cancellationToken);
    }

    public void RemovePlayer(byte ucid)
    {
        _currentPages.Remove(ucid);
    }
    
    public Task OpenShopAsync(
    Player player,
    CancellationToken cancellationToken)
    {
        return OpenPageAsync(
            player,
            _shopPage,
            cancellationToken);
    }

    public Task OpenVehicleCategoryAsync(
        Player player,
        VehicleShopCategory category,
        int page,
        CancellationToken cancellationToken)
    {
        return OpenPageAsync(
            player,
            new ShopVehiclesPage(
                category,
                _vehicleShopService,
                _ownershipService,
                _economyService,
                _databaseService,
                _sendMessage,
                page),
            cancellationToken);
    }
    public Task OpenVehicleCategoryAsync(
    Player player,
    VehicleShopCategory category,
    CancellationToken cancellationToken)
    {
        return OpenVehicleCategoryAsync(player, category, 0, cancellationToken);
    }
}