using LfsCruise.Core.Economy;
using LfsCruise.Core.Economy.Bank;
using LfsCruise.Core.Jobs;
using LfsCruise.Core.Players;
using LfsCruise.Core.UI.Menu.Pages;
using LfsCruise.Core.Vehicles;
using LfsCruise.Core.Vehicles.Market;
using LfsCruise.Core.Vehicles.Mods;
using LfsCruise.Core.Vehicles.Regitra;
using LfsCruise.Core.Vehicles.Shop;
using LfsCruise.Core.Vehicles.Starter;
using LfsCruise.Core.Vehicles.Garage;
using LfsCruise.Database;
using LfsCruise.Utils;

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
    private readonly ModNameService _modNameService; // NAUJA
    private readonly GarageService _garageService; // NAUJA

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
        ModNameService modNameService, // NAUJA
        GarageService garageService,
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
        _modNameService = modNameService; // NAUJA
        _garageService = garageService; // NAUJA
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
        {
            Console.WriteLine($"[MenuManager] No current page for player {player.Username} (UCID: {player.UCID} ");
            return;
        }

        Console.WriteLine($"[MenuManager] UCID={player.UCID} clickID={clickId} currentPage{page.GetType().Name}");

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

        var carName = await ResolveVehicleDisplayNameAsync(carCode, cancellationToken);

        await SendMessageAsync(player.UCID, $"^2Gavai pradine masina: ^7{carName}", cancellationToken);
        await CloseAsync(player, cancellationToken);
    }

    private async Task<string> ResolveVehicleDisplayNameAsync(string carCode, CancellationToken cancellationToken)
    {
        // 1) Jei mašina yra ir Parduotuvėje - naudojam ten priskirtą pavadinimą.
        var shopVehicle = _vehicleShopService.GetVehicleByCode(carCode);

        if (shopVehicle is not null)
            return shopVehicle.DisplayName;

        // 2) Kitu atveju tai mod'as - traukiam tikra pavadinimą iš lfs.net (su cache).
        var skinId = LfsCarCode.ToSkinId(carCode);

        return await _modNameService.ResolveNameAsync(skinId, cancellationToken);
    }

    public async Task OpenGarageAsync(Player player, int page, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Garage start {player.Username}");

        var carCodes = await _ownershipService.GetOwnedVehiclesAsync(player, cancellationToken);
        Console.WriteLine($"Garage: {player.Username} owns {carCodes.Count} cars.");

        var vehicles = new List<(string CarCode, string DisplayName)>();

        foreach (var carCode in carCodes)
        {
            Console.WriteLine($"Resolving display name for {carCode}...");
            var name = await ResolveVehicleDisplayNameAsync(carCode, cancellationToken);
            Console.WriteLine($"Resolved display name for {carCode}: {name}");
            vehicles.Add((carCode, name));
        }

        Console.WriteLine($"Garage: Opening page with {vehicles.Count}");
        await OpenPageAsync(player, new GaragePage(vehicles, page), cancellationToken);
        Console.WriteLine($"Garage: Opened");
    }

    public async Task OpenGarageSellChoiceAsync(Player player, string carCode, int originPage, CancellationToken cancellationToken)
    {
        var displayName = await ResolveVehicleDisplayNameAsync(carCode, cancellationToken);
        var shopPrice = _garageService.GetShopPrice(carCode);

        await OpenPageAsync(
            player,
            new GarageSellChoicePage(carCode, displayName, shopPrice, originPage),
            cancellationToken);
    }

    public async Task SellVehicleToServerAsync(Player player, string carCode, int originPage, CancellationToken cancellationToken)
    {
        var result = await _garageService.SellToServerAsync(player, carCode, cancellationToken);

        await SendMessageAsync(player.UCID, result.Success ? $"^2{result.Message}" : $"^1{result.Message}", cancellationToken);

        await OpenGarageAsync(player, originPage, cancellationToken);
    }

    public async Task ListVehicleOnMarketAsync(Player player, string carCode, string priceText, int originPage, CancellationToken cancellationToken)
    {
        if (!int.TryParse(priceText, out var price) || price <= 0)
        {
            await SendMessageAsync(player.UCID, "^1Neteisinga kaina.", cancellationToken);
            return;
        }

        var result = await _marketService.CreateListingForCarAsync(player, carCode, price, cancellationToken);

        await SendMessageAsync(player.UCID, result.Success ? $"^2{result.Message}" : $"^1{result.Message}", cancellationToken);

        await OpenGarageAsync(player, originPage, cancellationToken);
    }
}