using LfsCruise.Core.Players;
using LfsCruise.Core.UI.Menu;

namespace LfsCruise.Core.Economy.Bank;

public sealed class BankUiRefreshLoop
{
    private readonly PlayerManager _playerManager;
    private readonly MenuManager _menuManager;

    public BankUiRefreshLoop(PlayerManager playerManager, MenuManager menuManager)
    {
        _playerManager = playerManager;
        _menuManager = menuManager;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var player in _playerManager.GetAll())
            {
                await _menuManager.RefreshBankIfOpenAsync(player, cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}