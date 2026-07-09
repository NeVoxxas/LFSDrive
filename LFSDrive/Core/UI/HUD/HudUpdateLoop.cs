using LfsCruise.Core.Jobs.Police;
using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.HUD;

public sealed class HudUpdateLoop
{
    private readonly PlayerManager _playerManager;
    private readonly HudManager _hudManager;
    private readonly PoliceRadarService _policeRadarService;

    public HudUpdateLoop(PlayerManager playerManager, HudManager hudManager, PoliceRadarService policeRadarService)
    {
        _playerManager = playerManager;
        _hudManager = hudManager;
        _policeRadarService = policeRadarService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var player in _playerManager.GetAll())
            {
                try
                {
                    await _hudManager.UpdateAsync(player, cancellationToken);
                    await _policeRadarService.UpdateAsync(player, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HUD ERROR] UCID {player.UCID}: {ex}");
                }
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}