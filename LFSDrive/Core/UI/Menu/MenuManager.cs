using LfsCruise.Core.Economy;
using LfsCruise.Core.Economy.Bank;
using LfsCruise.Core.Jobs;
using LfsCruise.Core.Players;
using LfsCruise.Core.UI.Menu.Pages;
using LfsCruise.Core.Vehicles;
using LfsCruise.Core.Vehicles.Regitra;
using LfsCruise.Core.Vehicles.Shop;
using LfsCruise.Core.Vehicles.Market;
using LfsCruise.Core.Vehicles.Starter;
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
    private readonly JobManager _jobManager;
    private readonly JobService _jobService;
    private readonly JobsPage _jobsPage = new();
    private readonly BankService _bankService;
    private readonly RegitraService _regitraService;
    private readonly MarketService _marketService;
    private readonly StarterCarService _starterCarService; // NAUJA
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public MenuManager(
        MenuRenderer renderer,
        VehicleShopService vehicleShopService,
        VehicleOwnershipService ownershipService,
        EconomyService economyService,
        DatabaseService databaseService,
        JobManager jobManager,
        JobService jobService,
        BankService bankService,
        RegitraService regitraService,
        MarketService marketService,
        StarterCarService starterCarService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _renderer = renderer;
        _vehicleShopService = vehicleShopService;
        _ownershipService = ownershipService;
        _economyService = economyService;
        _databaseService = databaseService;
        _jobManager = jobManager;
        _jobService = jobService;
        _bankService = bankService;
        _regitraService = regitraService;
        _marketService = marketService;
        _starterCarService = starterCarService;
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
    
    // VEHICLE SHOP

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

    public Task OpenJobsAsync(Player player, CancellationToken cancellationToken)
    {
        return OpenPageAsync(player, _jobsPage, cancellationToken);
    }

    // JOBS

    public Task OpenJobDetailsAsync(
        Player player,
        JobType jobType,
        CancellationToken cancellationToken)
    {
        return OpenPageAsync(
            player,
            new JobDetailsPage(jobType, _jobService),
            cancellationToken);
    }

    public async Task JoinJobFromMenuAsync(
        Player player,
        JobType jobType,
        CancellationToken cancellationToken)
    {
        await _jobManager.StartJobAsync(player, jobType, cancellationToken);
        await OpenJobDetailsAsync(player, jobType, cancellationToken);
    }

    public async Task LeaveJobFromMenuAsync(
        Player player,
        CancellationToken cancellationToken)
    {
        await _jobManager.StopJobAsync(player, cancellationToken);
        await OpenJobsAsync(player, cancellationToken);
    }

    public Task OpenBankAsync(Player player, bool allowTransactions, bool showBackButton, CancellationToken cancellationToken)
    {
        return OpenPageAsync(player, new BankMenuPage(_bankService, allowTransactions, showBackButton), cancellationToken);
    }

    // BANK

    public async Task OpenBankHistoryAsync(
        Player player, bool allowTransactions, bool showBackButton, int page, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _bankService.GetHistoryAsync(player, page, cancellationToken);

        await OpenPageAsync(
            player,
            new BankHistoryPage(allowTransactions, showBackButton, page, items, totalCount),
            cancellationToken);
    }

    public async Task RefreshBankIfOpenAsync(Player player, CancellationToken cancellationToken)
    {
        if (_currentPages.TryGetValue(player.UCID, out var page) && page is BankMenuPage)
        {
            await _renderer.RenderAsync(player, page, cancellationToken);
        }
    }

    public async Task HandleTypeInAsync(Player player, byte clickId, string text, CancellationToken cancellationToken)
    {
        if (!_currentPages.TryGetValue(player.UCID, out var page))
            return;

        var context = new MenuContext { Player = player };

        await page.HandleTypeInAsync(this, context, clickId, text, cancellationToken);
    }

    public Task SendMessageAsync(byte ucid, string message, CancellationToken cancellationToken)
    {
        return _sendMessage(ucid, message, cancellationToken);
    }

    // REGITRA
    public Task OpenRegitraAsync(Player player, CancellationToken cancellationToken)
    {
        return OpenRegitraPageAsync(player, cancellationToken);
    }

    private async Task OpenRegitraPageAsync(Player player, CancellationToken cancellationToken)
    {
        VehicleDocuments? docs = null;

        if (!string.IsNullOrEmpty(player.CarName))
            docs = await _regitraService.GetDocumentsAsync(player, player.CarName, cancellationToken);

        await OpenPageAsync(player, new RegitraMenuPage(_regitraService, docs), cancellationToken);
    }

    public async Task BuyRegitraPlateAsync(Player player, string plateNumber, CancellationToken cancellationToken)
    {
        var result = await _regitraService.BuyPlateAsync(player, player.CarName, plateNumber, cancellationToken);
        await SendMessageAsync(player.UCID, result.Success ? $"^2{result.Message}" : $"^1{result.Message}", cancellationToken);
        await OpenRegitraPageAsync(player, cancellationToken);
    }

    public async Task ChangeRegitraPlateAsync(Player player, string plateNumber, CancellationToken cancellationToken)
    {
        var result = await _regitraService.ChangePlateAsync(player, player.CarName, plateNumber, cancellationToken);
        await SendMessageAsync(player.UCID, result.Success ? $"^2{result.Message}" : $"^1{result.Message}", cancellationToken);
        await OpenRegitraPageAsync(player, cancellationToken);
    }

    public async Task BuyRegitraInsuranceAsync(Player player, CancellationToken cancellationToken)
    {
        var result = await _regitraService.BuyInsuranceAsync(player, player.CarName, cancellationToken);
        await SendMessageAsync(player.UCID, result.Success ? $"^2{result.Message}" : $"^1{result.Message}", cancellationToken);
        await OpenRegitraPageAsync(player, cancellationToken);
    }

    public async Task BuyRegitraInspectionAsync(Player player, CancellationToken cancellationToken)
    {
        var result = await _regitraService.BuyInspectionAsync(player, player.CarName, cancellationToken);
        await SendMessageAsync(player.UCID, result.Success ? $"^2{result.Message}" : $"^1{result.Message}", cancellationToken);
        await OpenRegitraPageAsync(player, cancellationToken);
    }

    // AUTO TURGUS

    public async Task OpenMarketAsync(Player player, string categoryId, int page, CancellationToken cancellationToken)
    {
        var categories = _vehicleShopService.GetCategories();

        if (string.IsNullOrEmpty(categoryId))
            categoryId = categories.Count > 0 ? categories[0].Id : "D";

        var (listings, totalCount) = await _marketService.GetListingsAsync(categoryId, page, cancellationToken);

        await OpenPageAsync(
            player,
            new Pages.MarketPage(categories, categoryId, page, listings, totalCount),
            cancellationToken);
    }

    public async Task BuyMarketListingAsync(
        Player player, int listingId, string categoryId, int page, CancellationToken cancellationToken)
    {
        var result = await _marketService.BuyAsync(player, listingId, cancellationToken);

        await SendMessageAsync(player.UCID, result.Success ? $"^2{result.Message}" : $"^1{result.Message}", cancellationToken);

        await OpenMarketAsync(player, categoryId, page, cancellationToken);
    }

    // STARTER CARS

    public Task OpenStarterCarMenuAsync(Player player, CancellationToken cancellationToken)
    {
        return OpenPageAsync(player, new Pages.StarterCarPage(_starterCarService.Config), cancellationToken);
    }

    public async Task ChooseStarterCarAsync(Player player, string carCode, CancellationToken cancellationToken)
    {
        if (await _ownershipService.OwnsVehicleAsync(player, carCode, cancellationToken))
        {
            await SendMessageAsync(player.UCID, "^1Sia masina jau turi.", cancellationToken);
            await CloseAsync(player, cancellationToken);
            return;
        }

        await _ownershipService.AddVehicleAsync(player, carCode, cancellationToken);

        var carName = _vehicleShopService.GetVehicleByCode(carCode)?.DisplayName ?? carCode;

        await SendMessageAsync(player.UCID, $"^2Gavai pradine masina: ^7{carName}", cancellationToken);
        await CloseAsync(player, cancellationToken);
    }
}