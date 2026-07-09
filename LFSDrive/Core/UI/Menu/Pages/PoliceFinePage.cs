using LfsCruise.Core.Jobs.Police;
using LfsCruise.Core.Players;
using LfsCruise.Core.UI;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class PoliceFinePage : MenuPage
{
    private readonly Player _target;
    private readonly IReadOnlyList<PoliceViolation> _violations;
    private readonly HashSet<string> _selectedIds;

    public PoliceFinePage(Player target, IReadOnlyList<PoliceViolation> violations, HashSet<string> selectedIds)
    {
        _target = target;
        _violations = violations;
        _selectedIds = selectedIds;
    }

    public override string Title => $"Bauda: {_target.Username}";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var buttons = new List<MenuButton>
        {
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" }
        };

        byte clickId = ClickIds.Police.ViolationStart;

        foreach (var violation in _violations)
        {
            if (clickId > ClickIds.Police.ViolationEnd)
                break;

            var isSelected = _selectedIds.Contains(violation.Id);
            var check = isSelected ? "^2[x]" : "^7[ ]";

            buttons.Add(new MenuButton
            {
                ClickId = clickId++,
                Text = $"{check} ^7{violation.Name} ^8({violation.Fine}$)"
            });
        }

        var total = _violations.Where(v => _selectedIds.Contains(v.Id)).Sum(v => v.Fine);

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Police.FineConfirm,
            Text = total > 0 ? $"^2Israsyti bauda (^7{total}$^2)" : "^7Pasirink pazeidima",
            Enabled = total > 0
        });

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager, MenuContext context, byte clickId, CancellationToken cancellationToken)
    {
        var officer = context.Player;

        if (clickId == ClickIds.Menu.Back)
            return manager.OpenPoliceMenuAsync(officer, cancellationToken);

        if (clickId == ClickIds.Police.FineConfirm)
        {
            return _selectedIds.Count == 0
                ? Task.CompletedTask
                : manager.ConfirmPoliceFineAsync(officer, _target, _selectedIds, cancellationToken);
        }

        var index = clickId - ClickIds.Police.ViolationStart;

        if (index < 0 || index >= _violations.Count)
            return Task.CompletedTask;

        var violationId = _violations[index].Id;
        var newSelected = new HashSet<string>(_selectedIds);

        if (!newSelected.Add(violationId))
            newSelected.Remove(violationId);

        return manager.OpenPoliceFineAsync(officer, _target, newSelected, cancellationToken);
    }
}