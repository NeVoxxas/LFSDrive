using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.HUD;

public abstract class HudWidget
{
    public abstract byte ClickId { get; }
    public abstract byte Width { get; }
    public abstract string GetText(Player player);
    public virtual bool IsVisible(Player player) => true;
}