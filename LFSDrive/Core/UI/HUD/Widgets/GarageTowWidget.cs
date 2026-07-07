using LfsCruise.Core.Players;
using LfsCruise.Core.UI;
using LfsCruise.Core.UI.HUD;
using LfsCruise.Core.Vehicles.Tow;

namespace LfsCruise.Core.UI.HUD.Widgets;

public sealed class GarageTowWidget : HudWidget
{
    private readonly TowService _towService;

    public GarageTowWidget(TowService towService)
    {
        _towService = towService;
    }

    public override byte ClickId => ClickIds.Hud.GarageTow;
    public override byte Width => HudLayout.GarageTowWidth;

    public override string GetText(Player player)
    {
        return $"^7Garazas ^2{_towService.GetCurrentPrice(player)}$";
    }

    public override bool IsInteractive(Player player)
    {
        return player.Vehicle.Speed <= TowService.MaxStopSpeedKmh;
    }
}