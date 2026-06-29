using LfsCruise.Core.Players;
using LfsCruise.Core.UI.HUD;

namespace LfsCruise.Core.UI.Hud.Widgets;

public sealed class MenuWidget : HudWidget
{
    public override byte ClickId => ClickIds.Hud.Menu;
    public override byte Width => 10;

    public override string GetText(Player player)
    {
        return "^7MENU";
    }
}