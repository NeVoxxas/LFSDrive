using LfsCruise.Core.UI;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class MainMenuPage : MenuPage
{
    public override string Title => "LFS Drive";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        return
        [
            new() { ClickId = ClickIds.Menu.Profile, Text = "Profilis", Category = "ZAIDEJAS", Column = MenuColumn.Left },
            new() { ClickId = ClickIds.Menu.Statistics, Text = "Statistika", Category = "ZAIDEJAS", Column = MenuColumn.Right },
            new() { ClickId = ClickIds.Garage.MenuButton, Text = "Mano garazas", Category = "ZAIDEJAS", Column = MenuColumn.Left },

            new() { ClickId = ClickIds.Menu.Shop, Text = "Parduotuve", Category = "EKONOMIKA", Column = MenuColumn.Left },
            new() { ClickId = ClickIds.Bank.Menu, Text = "Bankas", Category = "EKONOMIKA", Column = MenuColumn.Right },
            new() { ClickId = ClickIds.Market.MenuButton, Text = "Auto turgus", Category = "EKONOMIKA", Column = MenuColumn.Left },

            new() { ClickId = ClickIds.Jobs.Menu, Text = "Darbai", Category = "DARBAS", Column = MenuColumn.Left },

            new() { ClickId = ClickIds.Menu.Top10, Text = "TOP 10", Category = "KITA", Column = MenuColumn.Left },
        ];
    }

    public override Task HandleClickAsync(MenuManager manager, MenuContext context, byte clickId, CancellationToken cancellationToken)
    {
        return clickId switch
        {
            ClickIds.Menu.Shop => manager.OpenShopAsync(context.Player, cancellationToken),
            ClickIds.Garage.MenuButton => manager.OpenGarageAsync(context.Player, 0, cancellationToken),
            ClickIds.Bank.Menu => manager.OpenBankAsync(context.Player, allowTransactions: false, showBackButton: true, cancellationToken),
            ClickIds.Market.MenuButton => manager.OpenMarketAsync(context.Player, string.Empty, 0, cancellationToken),
            ClickIds.Jobs.Menu => manager.OpenJobsAsync(context.Player, cancellationToken),
            _ => Task.CompletedTask
        };
    }
}