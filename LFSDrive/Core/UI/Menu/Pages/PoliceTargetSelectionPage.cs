using LfsCruise.Core.Jobs.Police;
using LfsCruise.Core.Players;
using LfsCruise.Core.UI;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class PoliceTargetSelectionPage : MenuPage
{
    private readonly IReadOnlyList<Player> _targets;
    private readonly PoliceAction _action;

    public PoliceTargetSelectionPage(IReadOnlyList<Player> targets, PoliceAction action)
    {
        _targets = targets;
        _action = action;
    }

    public override string Title => "Pasirink zaideja";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var buttons = new List<MenuButton>
        {
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" }
        };

        byte clickId = ClickIds.Police.TargetStart;

        foreach (var target in _targets)
        {
            if (clickId > ClickIds.Police.TargetEnd)
                break;

            buttons.Add(new MenuButton { ClickId = clickId++, Text = $"^7{target.Username}" });
        }

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager, MenuContext context, byte clickId, CancellationToken cancellationToken)
    {
        var officer = context.Player;

        if (clickId == ClickIds.Menu.Back)
            return manager.OpenPoliceMenuAsync(officer, cancellationToken);

        var index = clickId - ClickIds.Police.TargetStart;

        if (index < 0 || index >= _targets.Count)
            return Task.CompletedTask;

        return manager.OpenPoliceActionForTargetAsync(officer, _targets[index], _action, cancellationToken);
    }
}