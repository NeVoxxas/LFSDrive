using LfsCruise.Core.Players;
using LfsCruise.Core.UI.HUD;

namespace LfsCruise.Core.UI.HUD.Widgets;

public sealed class ServerWidget : HudWidget
{
    public override byte ClickId => 54;
    public override byte Width => HudLayout.ServerWidth;

    public override string GetText(Player player)
    {
        return "^7LFSDrive";
    }
}