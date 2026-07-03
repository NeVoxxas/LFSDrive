namespace LfsCruise.Core.Jobs.Delivery;

public sealed class DeliveryConfig
{
    public string CarCode { get; set; } = string.Empty;

    public DeliveryPoint Hq { get; set; } = new() { X = 0, Y = 0 };

    public DeliveryFareConfig Fare { get; set; } = new();

    public List<DeliveryPoint> Clients { get; set; } = new();

    public double ArrivalRadius { get; set; } = 10.0;

    public double ReverseChance { get; set; } = 0.3;

    public int MinCooldownMinutes { get; set; } = 1;
    public int MaxCooldownMinutes { get; set; } = 3;
}