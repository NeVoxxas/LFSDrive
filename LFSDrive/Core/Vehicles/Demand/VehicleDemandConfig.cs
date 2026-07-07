namespace LfsCruise.Core.Vehicles.Demand;

public sealed class VehicleDemandConfig
{
    public List<VehicleDemandTier> Tiers { get; set; } = new()
    {
        new VehicleDemandTier { MinOwners = 0, Multiplier = 1.00 },   // 0-3 savininkai - baze, jokio pokycio
        new VehicleDemandTier { MinOwners = 4, Multiplier = 1.10 },   // nuo 4-to pirkejo kaina pradeda kilti
        new VehicleDemandTier { MinOwners = 11, Multiplier = 1.20 },  // populiaru
        new VehicleDemandTier { MinOwners = 26, Multiplier = 1.35 },  // labai populiaru
        new VehicleDemandTier { MinOwners = 51, Multiplier = 1.50 }   // "hit" - riba, toliau nekyla
    };
}
