namespace LfsCruise.Core.Checkpoints;

public sealed class Checkpoint
{
    public required byte Ucid { get; init; }

    public required string Key { get; init; }

    public required double X { get; init; }

    public required double Y { get; init; }

    public double Radius { get; init; } = 10.0;
}