using LfsCruise.Core.UI;
using LfsCruise.Core.Vehicles.Starter;

namespace LfsCruise.Core.UI.Menu.Pages;

public sealed class StarterCarPage : MenuPage
{
    private readonly StarterCarConfig _config;

    public StarterCarPage(StarterCarConfig config)
    {
        _config = config;
    }

    public override string Title => "Pasirink pirma masina";

    public override IReadOnlyList<MenuButton> GetButtons(MenuContext context)
    {
        var buttons = new List<MenuButton>();

        byte clickId = ClickIds.Starter.CarStart;

        foreach (var car in _config.Cars)
        {
            if (clickId > ClickIds.Starter.CarEnd)
                break;

            buttons.Add(new MenuButton
            {
                ClickId = clickId++,
                Text = $"^2{car.DisplayName}"
            });
        }

        if (_config.Cars.Count == 0)
        {
            buttons.Add(new MenuButton
            {
                ClickId = ClickIds.Starter.CarStart,
                Text = "^1Pradines masinos nesukonfiguruotos",
                Enabled = false
            });
        }

        return buttons;
    }

    public override Task HandleClickAsync(
        MenuManager manager,
        MenuContext context,
        byte clickId,
        CancellationToken cancellationToken)
    {
        var index = clickId - ClickIds.Starter.CarStart;

        if (index < 0 || index >= _config.Cars.Count)
            return Task.CompletedTask;

        return manager.ChooseStarterCarAsync(context.Player, _config.Cars[index].CarCode, cancellationToken);
    }
}