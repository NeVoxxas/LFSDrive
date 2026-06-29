using LfsCruise.Core.Players;
using LfsCruise.Core.UI.Menu.Pages;
using LfsCruise.Core.Vehicles.Shop;

namespace LfsCruise.Core.UI.Menu;

public sealed class MenuManager
{
    private readonly MenuRenderer _renderer;
    private readonly Dictionary<byte, MenuPage> _currentPages = new();

    private readonly MainMenuPage _mainMenuPage = new();
    private readonly VehicleShopService _vehicleShopService;
    private readonly ShopCategoriesPage _shopPage;

    public MenuManager(
        MenuRenderer renderer,
        VehicleShopService vehicleShopService)
    {
        _renderer = renderer;
        _vehicleShopService = vehicleShopService;
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
        CancellationToken cancellationToken)
    {
        return OpenPageAsync(
            player,
            new ShopVehiclesPage(category, _vehicleShopService),
            cancellationToken);
    }
}