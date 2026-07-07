namespace LfsCruise.Core.Vehicles.Demand;

public sealed class VehicleDemandTier
{
    public int MinOwners { get; set; }
    public double Multiplier { get; set; } = 1.0;
}
