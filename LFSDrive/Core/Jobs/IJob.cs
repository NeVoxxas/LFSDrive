using LfsCruise.Core.Players;

namespace LfsCruise.Core.Jobs;

public interface IJob
{
    JobType Type { get; }

    string Name { get; }

    Task OnStartAsync(JobContext context, CancellationToken cancellationToken);

    Task OnStopAsync(JobContext context, CancellationToken cancellationToken);

    Task OnPlayerMoveAsync(JobContext context, CancellationToken cancellationToken);
}