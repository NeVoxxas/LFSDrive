using System.Numerics;

namespace LfsCruise.Core.Jobs.Taxi;

public sealed class TaxiMission
{
    public TaxiState State { get; set; } = TaxiState.Idle;

    public Vector3 Pickup { get; set; }

    public Vector3 Destination { get; set; }

    public int Reward { get; set; }

    public DateTime CooldownUntil { get; set; }
}