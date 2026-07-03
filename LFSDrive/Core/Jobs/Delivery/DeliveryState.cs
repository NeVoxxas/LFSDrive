namespace LfsCruise.Core.Jobs.Delivery;

public enum DeliveryState
{
    Idle = 0,
    DrivingToHqInitial = 1,
    DrivingToClient = 2,
    DrivingToPickupReverse = 3,
    DrivingToHqReturn = 4,
    Cooldown = 5
}