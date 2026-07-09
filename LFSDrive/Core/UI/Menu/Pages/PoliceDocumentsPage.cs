using LfsCruise.Core.Players;
using LfsCruise.Core.UI;
using LfsCruise.Core.Vehicles.Regitra;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class PoliceDocumentsPage : MenuPage
{
    private readonly Player _target;
    private readonly VehicleDocuments? _documents;

    public PoliceDocumentsPage(Player target, VehicleDocuments? documents)
    {
        _target = target;
        _documents = documents;
    }

    public override string Title => $"Dokumentai: {_target.Username}";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var buttons = new List<MenuButton>
        {
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" }
        };

        if (string.IsNullOrEmpty(_target.CarName))
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Police.ViolationStart,
                Text = "^1Zaidejas nera automobilyje.",
                Enabled = false
            });

            return buttons;
        }

        var docs = _documents;

        var plateText = docs?.HasPlate == true ? $"^2Numeriai: ^7{docs.PlateNumber}" : "^1Numeriu nera";
        buttons.Add(new MenuButton { ClickId = ClickIds.Police.ViolationStart, Text = plateText, Enabled = false });

        var insuranceText = docs?.HasValidInsurance == true
            ? $"^2Draudimas iki: ^7{docs.InsuranceExpiresAt:yyyy-MM-dd}"
            : "^1Draudimo nera";
        buttons.Add(new MenuButton { ClickId = ClickIds.Police.ViolationStart + 1, Text = insuranceText, Enabled = false });

        var inspectionText = docs?.HasValidInspection == true
            ? $"^2T.A galioja iki: ^7{docs.InspectionExpiresAt:yyyy-MM-dd}"
            : "^1T.A negalioja";
        buttons.Add(new MenuButton { ClickId = ClickIds.Police.ViolationStart + 2, Text = inspectionText, Enabled = false });

        var overall = docs?.IsValid == true ? "^2Dokumentai galioja" : "^1Dokumentai negalioja";
        buttons.Add(new MenuButton { ClickId = ClickIds.Police.ViolationStart + 3, Text = overall, Enabled = false });

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Police.DocsIssueFine,
            Text = "^2Israsyti bauda"
        });

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager, MenuContext context, byte clickId, CancellationToken cancellationToken)
    {
        var officer = context.Player;

        if (clickId == ClickIds.Menu.Back)
            return manager.OpenPoliceMenuAsync(officer, cancellationToken);

        if (clickId == ClickIds.Police.DocsIssueFine)
            return manager.OpenPoliceFineAsync(officer, _target, new HashSet<string>(), cancellationToken);

        return Task.CompletedTask;
    }
}