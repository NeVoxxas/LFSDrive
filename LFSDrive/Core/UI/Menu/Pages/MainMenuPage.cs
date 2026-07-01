using LfsCruise.Core.UI;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class MainMenuPage : MenuPage
{
    public override string Title => "LFS Drive";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        return
        [
            new() { ClickId = ClickIds.Menu.Profile, Text = "Profilis" },
            new() { ClickId = ClickIds.Menu.Shop, Text = "Parduotuve" },
            new() { ClickId = ClickIds.Bank.Menu, Text = "Bankas" },
            new() { ClickId = ClickIds.Menu.Top10, Text = "TOP 10" },
            new() { ClickId = ClickIds.Menu.Statistics, Text = "Statistika" },
            new() { ClickId = ClickIds.Jobs.Menu, Text = "Darbai" }
        ];
    }

    public override Task HandleClickAsync(MenuManager manager, MenuContext context, byte clickId, CancellationToken cancellationToken)
    {
        return clickId switch
        {
            ClickIds.Menu.Shop => manager.OpenShopAsync(context.Player, cancellationToken),
            ClickIds.Bank.Menu => manager.OpenBankAsync(context.Player, allowTransactions: false, showBackButton: true, cancellationToken),
            ClickIds.Jobs.Menu => manager.OpenJobsAsync(context.Player, cancellationToken),
            _ => Task.CompletedTask
        };
    }
}