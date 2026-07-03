using LfsCruise.Core.Jobs;
using LfsCruise.Core.Players;
using LfsCruise.Core.UI.HUD;

namespace LfsCruise.Core.UI.HUD.Widgets;

public sealed class JobStatusWidget : HudWidget
{
    private readonly JobService _jobService;

    public JobStatusWidget(JobService jobService)
    {
        _jobService = jobService;
    }

    public override byte ClickId => ClickIds.Hud.JobStatus;
    public override byte Width => HudLayout.JobStatusWidth;

    public override string GetText(Player player)
    {
        return _jobService.GetStatus(player);
    }
    public override bool IsVisible(Player player)
    {
        return !string.IsNullOrEmpty(_jobService.GetStatus(player));
    }
}