using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.HUD;

public abstract class HudWidget
{
    public abstract byte ClickId { get; }
    public abstract byte Width { get; }
    public abstract string GetText(Player player);
    public virtual bool IsVisible(Player player) => true;
    public virtual bool IsInteractive(Player player) => true;

    // NAUJA - jei nustatyta, widget'as "iskrenta" is auto-stack eilutes ir
    // piesamas TIKSLIAI sioje pozicijoje (kaip MenuButton.Left/Top).
    public virtual byte? Left => null;
    public virtual byte? Top => null;
}