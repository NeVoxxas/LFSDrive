using LfsCruise.Core.Jobs.Police;
using LfsCruise.Core.UI;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class PoliceMenuPage : MenuPage
{
    public override string Title => "^7Policijos meniu";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        return
        [
            new() { ClickId = ClickIds.Police.MenuFines, Text = "^2Israsyti bauda", Category = "POLICIJA", Column = MenuColumn.Left },
            new() { ClickId = ClickIds.Police.MenuDocs, Text = "^3Tikrinti dokumentus", Category = "POLICIJA", Column = MenuColumn.Right  },
            new() { ClickId = ClickIds.Police.MenuPursuits, Text = "^1Gaudomi zaidejai", Category = "POLICIJA", Column = MenuColumn.Left  }
        ];
    }

    public override Task HandleClickAsync(
        MenuManager manager, MenuContext context, byte clickId, CancellationToken cancellationToken)
    {
        var player = context.Player;

        if (clickId == ClickIds.Police.MenuFines)
            return manager.OpenPoliceTargetSelectionAsync(player, PoliceAction.Fine, cancellationToken);

        if (clickId == ClickIds.Police.MenuDocs)
            return manager.OpenPoliceTargetSelectionAsync(player, PoliceAction.Documents, cancellationToken);

        if (clickId == ClickIds.Police.MenuPursuits)
            return manager.OpenPolicePursuitsAsync(player, cancellationToken);

        return Task.CompletedTask;
    }
}