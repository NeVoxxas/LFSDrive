using LfsCruise.Core.Players;
using LfsCruise.Database;

namespace LfsCruise.Core.Economy.Bank;

public readonly struct BankInterestStatus
{
    public bool IsPaused { get; init; }
    public TimeSpan Remaining { get; init; }

    public static BankInterestStatus Paused() => new() { IsPaused = true, Remaining = TimeSpan.Zero };

    public static BankInterestStatus Active(TimeSpan remaining) => new()
    {
        IsPaused = false,
        Remaining = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining
    };
}

public sealed class BankService
{
    public const double InterestRate = 0.05; // 5%
    public static readonly TimeSpan InterestInterval = TimeSpan.FromMinutes(60);

    private readonly BankTransactionStorage _transactionStorage;
    private readonly DatabaseService _databaseService;

    public BankService(BankTransactionStorage transactionStorage, DatabaseService databaseService)
    {
        _transactionStorage = transactionStorage;
        _databaseService = databaseService;
    }

    public async Task<bool> DepositAsync(Player player, int amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0 || player.Data.Money < amount)
            return false;

        player.Data.Money -= amount;
        player.Data.Bank += amount;

        await _databaseService.SavePlayerAsync(player, cancellationToken);
        await LogAsync(player, BankTransactionType.Deposit, amount, cancellationToken);

        return true;
    }

    public async Task<bool> WithdrawAsync(Player player, int amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0 || player.Data.Bank < amount)
            return false;

        player.Data.Bank -= amount;
        player.Data.Money += amount;

        await _databaseService.SavePlayerAsync(player, cancellationToken);
        await LogAsync(player, BankTransactionType.Withdraw, amount, cancellationToken);

        return true;
    }

    public Task<(IReadOnlyList<BankTransaction> Items, int TotalCount)> GetHistoryAsync(
        Player player, int page, CancellationToken cancellationToken = default)
    {
        return _transactionStorage.GetPageAsync(player.Data.Id, page, 5, cancellationToken);
    }

    // Nauja: grąžina kiek liko laiko iki kitų palūkanų, arba kad jos sustabdytos.
    public BankInterestStatus GetInterestStatus(Player player)
    {
        if (player.Data.Bank < 0)
            return BankInterestStatus.Paused();

        if (player.Data.LastInterestAt is null)
            return BankInterestStatus.Active(InterestInterval);

        var elapsed = DateTime.UtcNow - player.Data.LastInterestAt.Value;
        var remaining = InterestInterval - elapsed;

        return BankInterestStatus.Active(remaining);
    }

    public async Task TryApplyInterestAsync(Player player, CancellationToken cancellationToken = default)
    {
        if (player.Data.Bank < 0)
            return;

        var now = DateTime.UtcNow;

        if (player.Data.LastInterestAt is null)
        {
            player.Data.LastInterestAt = now;
            await _databaseService.SavePlayerAsync(player, cancellationToken);
            return;
        }

        if (now - player.Data.LastInterestAt.Value < InterestInterval)
            return;

        var interest = (int)Math.Round(player.Data.Bank * InterestRate);

        player.Data.LastInterestAt = now;

        if (interest <= 0)
        {
            await _databaseService.SavePlayerAsync(player, cancellationToken);
            return;
        }

        player.Data.Bank += interest;

        await _databaseService.SavePlayerAsync(player, cancellationToken);
        await LogAsync(player, BankTransactionType.Interest, interest, cancellationToken);
    }

    private Task LogAsync(Player player, BankTransactionType type, int amount, CancellationToken cancellationToken)
    {
        return _transactionStorage.AddAsync(new BankTransaction
        {
            PlayerId = player.Data.Id,
            Type = type,
            Amount = amount,
            BalanceAfter = player.Data.Bank,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }
}