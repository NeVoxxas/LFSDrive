namespace LfsCruise.Core.Vehicles.Demand;

public sealed class VehicleDemandState
{
    public double Multiplier { get; set; } = 1.0;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
