using LfsCruise.Core.Jobs.Police;
using LfsCruise.Core.Players;
using LfsCruise.Core.UI;
using LfsCruise.Core.UI.HUD;

namespace LfsCruise.Core.UI.HUD.Widgets;

public sealed class WantedWidget : HudWidget
{
    private readonly PursuitService _pursuitService;

    public WantedWidget(PursuitService pursuitService)
    {
        _pursuitService = pursuitService;
    }

    public override byte ClickId => ClickIds.Hud.Wanted;
    public override byte Width => HudLayout.WantedWidth;

    // NAUJA - fiksuota pozicija (paznymeta raudonai) vietoj auto-stack eilutes
    public override byte? Left => 140;
    public override byte? Top => 83;

    public override string GetText(Player player)
    {
        var info = _pursuitService.GetPursuitInfo(player.UCID);

        if (info is null)
            return string.Empty;

        return $"^1Gaudomas! ^7Lygis: ^1{info.Value.Level}/5 ^7| ^3{info.Value.OfficerName}";
    }

    public override bool IsVisible(Player player)
    {
        return _pursuitService.IsWanted(player.UCID);
    }

    public override bool IsInteractive(Player player) => false;
}