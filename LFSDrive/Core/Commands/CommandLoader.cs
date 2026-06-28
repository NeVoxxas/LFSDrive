using LfsCruise.Core.Economy;

namespace LfsCruise.Core.Commands;

public static class CommandLoader
{
    public static void RegisterAll(
        CommandManager commandManager,
        EconomyService economyService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        commandManager.Register(new HelpCommand(sendMessage));
        commandManager.Register(new IdCommand(sendMessage));
        commandManager.Register(new MoneyCommand(sendMessage));
        commandManager.Register(new DepositCommand(economyService, sendMessage));
        commandManager.Register(new WithdrawCommand(economyService, sendMessage));
    }
}