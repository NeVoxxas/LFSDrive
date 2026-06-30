namespace LfsCruise.Core.Jobs.Taxi;

public sealed class TaxiPointConfig
{
    public List<TaxiPoint> Pickup { get; set; } = new();

    public List<TaxiPoint> Destination { get; set; } = new();
}