using LfsCruise.Core.Players;
using LfsCruise.Core.UI.HUD;

namespace LfsCruise.Core.UI.HUD.Widgets;

public sealed class DiscordWidget : HudWidget
{
    public override byte ClickId => 53;
    public override byte Width => HudLayout.DiscordWidth;

    public override string GetText(Player player)
    {
        return "^7Linkas";
    }
}