using LfsCruise.Core.Players;

namespace LfsCruise.Core.Economy.Bank;

public sealed class BankInterestLoop
{
    private readonly PlayerManager _playerManager;
    private readonly BankService _bankService;

    public BankInterestLoop(PlayerManager playerManager, BankService bankService)
    {
        _playerManager = playerManager;
        _bankService = bankService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var player in _playerManager.GetAll())
            {
                await _bankService.TryApplyInterestAsync(player, cancellationToken);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }
}