using LfsCruise.Core.Checkpoints;
using LfsCruise.Core.Economy;
using LfsCruise.Core.GPS;
using LfsCruise.Core.Players;
using LfsCruise.Database;

namespace LfsCruise.Core.Jobs;

public sealed class JobContext
{
    public required Player Player { get; init; }

    public required JobService JobService { get; init; }

    public required CheckpointManager CheckpointManager { get; init; }

    public required GpsService GpsService { get; init; }

    public required EconomyService EconomyService { get; init; }

    public required DatabaseService DatabaseService { get; init; }

    public required Func<byte, string, CancellationToken, Task> SendMessage { get; init; }

    public required Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> SendButton { get; init; }

    public required Func<byte, byte, byte, CancellationToken, Task> DeleteButtons { get; init; }
}