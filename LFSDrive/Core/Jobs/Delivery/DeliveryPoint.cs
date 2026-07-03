namespace LfsCruise.Core.Jobs.Delivery;

public sealed class DeliveryPoint
{
    public string Name { get; set; } = string.Empty;
    public required double X { get; set; }
    public required double Y { get; set; }
}