using LfsCruise.Core.Jobs;
using LfsCruise.Core.Players;
using LfsCruise.Core.Progression;
using LfsCruise.Core.UI.Hud.Widgets;
using LfsCruise.Core.UI.HUD.Widgets;
using LfsCruise.Core.Vehicles.Tow;

namespace LfsCruise.Core.UI.HUD;

public sealed class HudManager
{
    private readonly HudRenderer _renderer;
    private readonly List<HudWidget> _widgets;

    public HudManager(
        HudRenderer renderer,
        ProgressionService progressionService,
        JobService jobService,
        TowService towService) // NAUJA
    {
        _renderer = renderer;

        _widgets =
        [
            new LicenseWidget(progressionService),
            new DistanceWidget(),
            new MoneyWidget(),
            new JobStatusWidget(jobService),
            new GarageTowWidget(towService), // NAUJA
            new ServerWidget(),
            new DiscordWidget(),
            new MenuWidget()
        ];
    }

    public Task UpdateAsync(Player player, CancellationToken cancellationToken = default)
    {
        return _renderer.RenderAsync(player, _widgets, cancellationToken);
    }

    public void RemovePlayer(byte ucid)
    {
        _renderer.RemovePlayer(ucid);
    }
}