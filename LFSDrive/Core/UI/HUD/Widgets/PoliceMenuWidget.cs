using LfsCruise.Core.Jobs;
using LfsCruise.Core.Players;
using LfsCruise.Core.UI;
using LfsCruise.Core.UI.HUD;

namespace LfsCruise.Core.UI.HUD.Widgets;

public sealed class PoliceMenuWidget : HudWidget
{
    private readonly JobService _jobService;

    public PoliceMenuWidget(JobService jobService)
    {
        _jobService = jobService;
    }

    public override byte ClickId => ClickIds.Hud.PoliceMenu;
    public override byte Width => HudLayout.PoliceMenuWidth;

    public override byte? Left => 165;
    public override byte? Top => 160;

    public override string GetText(Player player) => "^3PD Meniu";

    public override bool IsVisible(Player player)
    {
        return _jobService.GetJob(player) == JobType.Police;
    }
}