namespace LfsCruise.Core;

public sealed class ServerConfig
{
    public string Host { get; init; } = "188.122.74.156";

    public int Port { get; init; } = 59596;

    public string AdminPassword { get; init; } = "346588030";

    public string InSimName { get; init; } = "LFSDrive";

    public char Prefix { get; init; } = '!';

    public int Interval { get; init; } = 1000;
}
