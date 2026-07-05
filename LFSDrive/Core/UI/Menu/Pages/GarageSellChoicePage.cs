using LfsCruise.Core.UI;
using LfsCruise.Core.Vehicles.Garage;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class GarageSellChoicePage : MenuPage
{
    private const byte PriceMaxChars = 8;

    private readonly string _carCode;
    private readonly string _displayName;
    private readonly int? _shopPrice;
    private readonly int _originPage;

    public GarageSellChoicePage(string carCode, string displayName, int? shopPrice, int originPage)
    {
        _carCode = carCode;
        _displayName = displayName;
        _shopPrice = shopPrice;
        _originPage = originPage;
    }

    public override string Title => _displayName;

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var buttons = new List<MenuButton>
        {
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" }
        };

        if (_shopPrice is null)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Garage.SellToServer,
                Text = "^1Si transporto priemone neturi nustatytos kainos.",
                Enabled = false
            });

            return buttons;
        }

        var serverPrice = GarageService.GetServerSellPrice(_shopPrice.Value);
        var maxMarketPrice = GarageService.GetMaxMarketPrice(_shopPrice.Value);

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Garage.SellToServer,
            Text = $"^3Parduoti serveriui (^7{serverPrice}$^3)"
        });

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Garage.SellOnMarket,
            Text = $"^2Ideti i auto turga (iki ^7{maxMarketPrice}$^2)",
            TypeIn = PriceMaxChars
        });

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Menu.Back)
            return manager.OpenGarageAsync(context.Player, _originPage, cancellationToken);

        if (clickId == ClickIds.Garage.SellToServer)
            return manager.SellVehicleToServerAsync(context.Player, _carCode, _originPage, cancellationToken);

        return Task.CompletedTask;
    }

    public override Task HandleTypeInAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        string text,
        CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Garage.SellOnMarket)
            return manager.ListVehicleOnMarketAsync(context.Player, _carCode, text, _originPage, cancellationToken);

        return Task.CompletedTask;
    }
}