using LfsCruise.Core.Players;
using LfsCruise.Database;

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

    public Task<bool> AddMoneyAsync(Player player, int amount)
    {
        var result = AddMoney(player, amount);
        return Task.FromResult(result);
    }

    public Task<bool> RemoveMoneyAsync(Player player, int amount)
    {
        var result = RemoveMoney(player, amount);
        return Task.FromResult(result);
    }   

}