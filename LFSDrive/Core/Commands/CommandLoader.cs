using LfsCruise.Core.Commands.Admin;
using LfsCruise.Core.Commands.Admin.Zones;
using LfsCruise.Core.Commands.Economy;
using LfsCruise.Core.Commands.General;
using LfsCruise.Core.Commands.Jobs;
using LfsCruise.Core.Jobs;
using LfsCruise.Core.Economy;
using LfsCruise.Core.Progression;
using LfsCruise.Core.Players;
using LfsCruise.Core.World;

namespace LfsCruise.Core.Commands;

public static class CommandLoader
{
    public static void RegisterAll(
        CommandManager commandManager,
        EconomyService economyService,
        ZoneService zoneService,
        ProgressionService progressionService,
        JobManager jobManager,
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
        commandManager.Register(new RemoveZoneCommand(zoneService, sendMessage));
        commandManager.Register(new ShowLicCommand(progressionService, sendMessage));
        commandManager.Register(new JobsCommand(sendMessage));
        commandManager.Register(new JoinJobCommand(jobManager, sendMessage));
        commandManager.Register(new LeaveJobCommand(jobManager));
    }
}