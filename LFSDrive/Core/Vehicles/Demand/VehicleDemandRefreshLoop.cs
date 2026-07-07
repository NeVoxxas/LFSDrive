namespace LfsCruise.Core.Vehicles.Demand;

public sealed class VehicleDemandRefreshLoop
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(3);

    private readonly VehicleDemandService _demandService;

    public VehicleDemandRefreshLoop(VehicleDemandService demandService)
    {
        _demandService = demandService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _demandService.RefreshAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Vehicle demand refresh failed: {ex.Message}");
            }

            await Task.Delay(Interval, cancellationToken);
        }
    }
}
