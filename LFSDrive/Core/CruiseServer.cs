using LfsCruise.InSim.Networking;

namespace LfsCruise.Core;

public sealed class CruiseServer
{
    private readonly ServerConfig _config;

    public CruiseServer(ServerConfig config)
    {
        _config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var inSimClient = new InSimClient(_config);

        await inSimClient.ConnectAsync(cancellationToken);
    }
}
