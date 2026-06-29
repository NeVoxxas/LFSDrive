using LfsCruise.Core.Players;
using LfsCruise.Core.UI.HUD;

namespace LfsCruise.Core.UI.HUD.Widgets;

public sealed class MoneyWidget : HudWidget
{
    public override byte ClickId => 52;
    public override byte Width => HudLayout.MoneyWidth;

    public override string GetText(Player player)
    {
        return $"^2${player.Data.Money:F0}";
    }
}