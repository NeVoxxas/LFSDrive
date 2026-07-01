namespace LfsCruise.Core.Economy.Bank;

public sealed class BankTransaction
{
    public int Id { get; init; }
    public int PlayerId { get; init; }
    public BankTransactionType Type { get; init; }
    public int Amount { get; init; }
    public int BalanceAfter { get; init; }
    public DateTime CreatedAt { get; init; }
}