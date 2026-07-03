namespace LfsCruise.Core.Jobs.Delivery;

public sealed class DeliveryMission
{
    public DeliveryState State { get; set; } = DeliveryState.Idle;

    public int RemainingDeliveries { get; set; }

    public DeliveryPoint Origin { get; set; } = null!;
    public DeliveryPoint Target { get; set; } = null!;

    public DateTime CooldownUntil { get; set; }
}