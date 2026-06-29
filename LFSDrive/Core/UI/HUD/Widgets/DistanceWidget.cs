using LfsCruise.Core.Players;
using LfsCruise.Core.UI.HUD;

namespace LfsCruise.Core.UI.HUD.Widgets;

public sealed class DistanceWidget : HudWidget
{
    public override byte ClickId => 51;
    public override byte Width => HudLayout.DistanceWidth;

    public override string GetText(Player player)
    {
        return $"^7{player.Data.DrivenDistance:F1} km";
    }
}