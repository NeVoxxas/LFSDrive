using LfsCruise.Core.UI;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class GaragePage : MenuPage
{
    private const int PageSize = 5;

    private readonly IReadOnlyList<(string CarCode, string DisplayName)> _vehicles;
    private readonly int _page;

    public GaragePage(IReadOnlyList<(string CarCode, string DisplayName)> vehicles, int page)
    {
        _vehicles = vehicles;
        _page = Math.Max(0, page);
    }

    public override string Title => "Mano garazas";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var buttons = new List<MenuButton>
        {
            new() { ClickId = ClickIds.Menu.Back, Text = "^7< Atgal" }
        };

        var totalPages = Math.Max(1, (int)Math.Ceiling(_vehicles.Count / (double)PageSize));
        var currentPage = Math.Clamp(_page, 0, totalPages - 1);

        var pageVehicles = _vehicles
            .Skip(currentPage * PageSize)
            .Take(PageSize)
            .ToList();

        if (_vehicles.Count == 0)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Garage.VehicleStart,
                Text = "^7Tu dar neturi transporto priemoniu",
                Enabled = false
            });

            return buttons;
        }

        byte clickId = ClickIds.Garage.VehicleStart;

        foreach (var vehicle in pageVehicles)
        {
            buttons.Add(new MenuButton
            {
                ClickId = clickId++,
                Text = $"^2{vehicle.DisplayName} ^7- ^1Parduoti"
            });
        }

        if (currentPage > 0)
        {
            buttons.Add(new MenuButton { ClickId = ClickIds.Garage.PrevPage, Text = "^7< Ankstesnis" });
        }

        buttons.Add(new MenuButton
        {
            ClickId = ClickIds.Garage.PageInfo,
            Text = $"^7Puslapis {currentPage + 1}/{totalPages}",
            Enabled = false
        });

        if (currentPage < totalPages - 1)
        {
            buttons.Add(new MenuButton { ClickId = ClickIds.Garage.NextPage, Text = "^7Kitas >" });
        }

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        if (clickId == ClickIds.Menu.Back)
            return manager.OpenMainMenuAsync(context.Player, cancellationToken);

        if (clickId == ClickIds.Garage.PrevPage)
            return manager.OpenGarageAsync(context.Player, _page - 1, cancellationToken);

        if (clickId == ClickIds.Garage.NextPage)
            return manager.OpenGarageAsync(context.Player, _page + 1, cancellationToken);

        if (clickId == ClickIds.Garage.PageInfo)
            return Task.CompletedTask;

        var totalPages = Math.Max(1, (int)Math.Ceiling(_vehicles.Count / (double)PageSize));
        var currentPage = Math.Clamp(_page, 0, totalPages - 1);

        var indexOnPage = clickId - ClickIds.Garage.VehicleStart;

        if (indexOnPage < 0 || indexOnPage >= PageSize)
            return Task.CompletedTask;

        var vehicleIndex = (currentPage * PageSize) + indexOnPage;

        if (vehicleIndex < 0 || vehicleIndex >= _vehicles.Count)
            return Task.CompletedTask;

        var vehicle = _vehicles[vehicleIndex];

        return manager.OpenGarageSellChoiceAsync(context.Player, vehicle.CarCode, currentPage, cancellationToken);
    }
}