namespace LfsCruise.Core.Jobs.Delivery;

public sealed class DeliveryFareConfig
{
    public int BaseFare { get; set; } = 80;
    public int PricePerKm { get; set; } = 250;
    public int MinReward { get; set; } = 150;
    public int MaxReward { get; set; } = 2500;
}