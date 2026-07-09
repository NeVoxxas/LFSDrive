using LfsCruise.Core.UI;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class PolicePursuitsPage : MenuPage
{
    private readonly IReadOnlyList<(string TargetName, int Level, string OfficerNames)> _pursuits;

    public PolicePursuitsPage(IReadOnlyList<(string TargetName, int Level, string OfficerNames)> pursuits)
    {
        _pursuits = pursuits;
    }

    public override string Title => "Gaudomi zaidejai";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var buttons = new List<MenuButton>
        {
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" }
        };

        if (_pursuits.Count == 0)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Police.PursuitEntryStart,
                Text = "^7Siuo metu niekas negaudomas",
                Enabled = false
            });

            return buttons;
        }

        byte clickId = ClickIds.Police.PursuitEntryStart;

        foreach (var pursuit in _pursuits)
        {
            if (clickId > ClickIds.Police.PursuitEntryEnd)
                break;

            buttons.Add(new MenuButton
            {
                ClickId = clickId++,
                Text = $"^1{pursuit.TargetName} ^7- Lygis {pursuit.Level}/5 ^8({pursuit.OfficerNames})",
                Enabled = false
            });
        }

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager, MenuContext context, byte clickId, CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Menu.Back)
            return manager.OpenPoliceMenuAsync(context.Player, cancellationToken);

        return Task.CompletedTask;
    }
}