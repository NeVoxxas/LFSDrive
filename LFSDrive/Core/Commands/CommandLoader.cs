using LfsCruise.Core.Economy;
using LfsCruise.Core.World;

namespace LfsCruise.Core.Commands;

public static class CommandLoader
{
    public static void RegisterAll(
        CommandManager commandManager,
        EconomyService economyService,
        ZoneService zoneService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        commandManager.Register(new HelpCommand(sendMessage));
        commandManager.Register(new IdCommand(sendMessage));
        commandManager.Register(new MoneyCommand(sendMessage));
        commandManager.Register(new DepositCommand(economyService, sendMessage));
        commandManager.Register(new WithdrawCommand(economyService, sendMessage));
        commandManager.Register(new PosCommand(sendMessage));
        commandManager.Register(new CreateZoneCommand(zoneService, sendMessage));
        commandManager.Register(new SaveZonesCommand(zoneService, sendMessage));
        commandManager.Register(new ReloadZonesCommand(zoneService, sendMessage));
    }
}