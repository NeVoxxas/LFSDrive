using LfsCruise.Core.UI;
using LfsCruise.Core.Vehicles.Regitra;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class RegitraMenuPage : MenuPage
{
    private readonly RegitraService _regitraService;
    private readonly VehicleDocuments? _documents;

    public RegitraMenuPage(RegitraService regitraService, VehicleDocuments? documents)
    {
        _regitraService = regitraService;
        _documents = documents;
    }

    public override string Title => "Regitra";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var player = context.Player;

        var buttons = new List<MenuButton>
        {
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" }
        };

        if (string.IsNullOrEmpty(player.CarName))
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Regitra.Status,
                Text = "^1Nesi automobilyje.",
                Enabled = false
            });

            return buttons;
        }

        var docs = _documents;
        var config = _regitraService.Config;

        var plateText = docs?.HasPlate == true
            ? $"^2Numeriai: ^7{docs.PlateNumber}"
            : "^1Numeriu nera";

        buttons.Add(new MenuButton { ClickId = ClickIds.Regitra.PlateStatus, Text = plateText, Enabled = false });

        if (docs?.HasPlate != true)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Regitra.BuyPlate,
                Text = $"^2Pirkti numerius (^7{config.PlatePrice}$^2)",
                TypeIn = 8
            });
        }
        else
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Regitra.ChangePlate,
                Text = $"^3Keisti numerius (^7{config.PlateChangePrice}$^3)",
                TypeIn = 8
            });
        }

        var insuranceText = docs?.HasValidInsurance == true
            ? $"^2Draudimas iki: ^7{docs.InsuranceExpiresAt:yyyy-MM-dd}"
            : "^1Draudimo nera";

        buttons.Add(new MenuButton { ClickId = ClickIds.Regitra.InsuranceStatus, Text = insuranceText, Enabled = false });
        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Regitra.BuyInsurance,
            Text = $"^2Pirkti draudima (^7{config.InsurancePrice}$^2, {config.InsuranceDurationDays}d.)"
        });

        var inspectionText = docs?.HasValidInspection == true
            ? $"^2T.A galioja iki: ^7{docs.InspectionExpiresAt:yyyy-MM-dd}"
            : "^1T.A negalioja";

        buttons.Add(new MenuButton { ClickId = ClickIds.Regitra.InspectionStatus, Text = inspectionText, Enabled = false });
        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Regitra.BuyInspection,
            Text = $"^2Praeiti T.A (^7{config.InspectionPrice}$^2, {config.InspectionDurationMonths} men.)"
        });

        var overall = docs?.IsValid == true ? "^2Dokumentai galioja" : "^1Dokumentai negalioja";
        buttons.Add(new MenuButton { ClickId = ClickIds.Regitra.Status, Text = overall, Enabled = false });

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager, MenuContext context, byte clickId, CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Menu.Back)
            return manager.OpenMainMenuAsync(context.Player, cancellationToken);

        if (clickId == ClickIds.Regitra.BuyInsurance)
            return manager.BuyRegitraInsuranceAsync(context.Player, cancellationToken);

        if (clickId == ClickIds.Regitra.BuyInspection)
            return manager.BuyRegitraInspectionAsync(context.Player, cancellationToken);

        return Task.CompletedTask;
    }

    public override Task HandleTypeInAsync(
        MenuManager manager, MenuContext context, byte clickId, string text, CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Regitra.BuyPlate)
            return manager.BuyRegitraPlateAsync(context.Player, text, cancellationToken);

        if (clickId == ClickIds.Regitra.ChangePlate)
            return manager.ChangeRegitraPlateAsync(context.Player, text, cancellationToken);

        return Task.CompletedTask;
    }
}