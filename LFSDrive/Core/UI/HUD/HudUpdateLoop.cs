using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.HUD;

public sealed class HudUpdateLoop
{
    private readonly PlayerManager _playerManager;
    private readonly HudManager _hudManager;

    public HudUpdateLoop(PlayerManager playerManager, HudManager hudManager)
    {
        _playerManager = playerManager;
        _hudManager = hudManager;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            //Console.WriteLine($"HUD players: {_playerManager.GetAll().Count}");

            foreach (var player in _playerManager.GetAll())
            {
                await _hudManager.UpdateAsync(player, cancellationToken);

            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}