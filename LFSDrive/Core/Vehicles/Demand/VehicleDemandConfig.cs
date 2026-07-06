namespace LfsCruise.Core.Vehicles.Demand;

public sealed class VehicleDemandConfig
{
    public Dictionary<string, VehicleDemandState> States { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
