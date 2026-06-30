using LfsCruise.Core.Jobs;
using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands.Jobs;

public sealed class LeaveJobCommand : ICommand
{
    private readonly JobManager _jobManager;

    public LeaveJobCommand(JobManager jobManager)
    {
        _jobManager = jobManager;
    }

    public string Name => "leavejob";
    public string Description => "Iseiti is darbo";
    public bool AdminOnly => false;

    public Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        return _jobManager.StopJobAsync(player, cancellationToken);
    }
}