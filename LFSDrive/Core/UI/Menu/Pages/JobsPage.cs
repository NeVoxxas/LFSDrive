using LfsCruise.Core.Jobs;
using LfsCruise.Core.UI;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class JobsPage : MenuPage
{
    public override string Title => "Darbai";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        return
        [
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" },
            new() { ClickId = ClickIds.Jobs.Taxi, Text = "^3Taxi" },
            new() { ClickId = ClickIds.Jobs.Delivery, Text = "^2DPD" }
        ];
    }

    public override Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        return clickId switch
        {
            ClickIds.Menu.Back => manager.OpenMainMenuAsync(context.Player, cancellationToken),
            ClickIds.Jobs.Taxi => manager.OpenJobDetailsAsync(context.Player, JobType.Taxi, cancellationToken),
            ClickIds.Jobs.Delivery => manager.OpenJobDetailsAsync(context.Player, JobType.Delivery, cancellationToken),
            _ => Task.CompletedTask
        };
    }
}