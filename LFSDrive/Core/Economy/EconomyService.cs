using LfsCruise.Core.Players;

namespace LfsCruise.Core.Economy;

public sealed class EconomyService
{
    public bool AddMoney(Player player, int amount)
    {
        if (amount <= 0)
            return false;

        player.Data.Money += amount;
        return true;
    }

    public bool RemoveMoney(Player player, int amount)
    {
        if (amount <= 0)
            return false;

        if (player.Data.Money < amount)
            return false;

        player.Data.Money -= amount;
        return true;
    }

    public void SetMoney(Player player, int amount)
    {
        player.Data.Money = Math.Max(0, amount);
    }

    public bool Deposit(Player player, int amount)
    {
        if (!RemoveMoney(player, amount))
            return false;

        player.Data.Bank += amount;
        return true;
    }

    public bool Withdraw(Player player, int amount)
    {
        if (amount <= 0)
            return false;

        if (player.Data.Bank < amount)
            return false;

        player.Data.Bank -= amount;
        player.Data.Money += amount;
        return true;
    }
}