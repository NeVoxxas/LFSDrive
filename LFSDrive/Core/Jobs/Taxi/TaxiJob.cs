using LfsCruise.Core.Players;

namespace LfsCruise.Core.Jobs.Taxi;

public sealed class TaxiJob : IJob
{
    private readonly Dictionary<byte, TaxiMission> _missions = new();
    private readonly TaxiPointConfig _points;
    private readonly Random _random = new();

    private const double PickupDistance = 5.0;
    private const double DestinationDistance = 5.0;
    private const double MaxStopSpeed = 1.0;

    private const int MinCooldownMinutes = 1;
    private const int MaxCooldownMinutes = 4;

    public TaxiJob(TaxiPointStorage storage)
    {
        _points = storage.Load();
    }

    public JobType Type => JobType.Taxi;

    public string Name => "Taxi";

    public async Task OnStartAsync(JobContext context, CancellationToken cancellationToken)
    {
        if (_points.Pickup.Count == 0 || _points.Destination.Count == 0)
        {
            await context.SendMessage(
                context.Player.UCID,
                "^1Taxi taskai nesukonfiguruoti.",
                cancellationToken);

            return;
        }

        var pickup = GetRandomPoint(_points.Pickup);
        var destination = GetRandomPoint(_points.Destination);

        var mission = new TaxiMission
        {
            State = TaxiState.DrivingToPickup,
            Pickup = new System.Numerics.Vector3((float)pickup.X, (float)pickup.Y, 0),
            Destination = new System.Numerics.Vector3((float)destination.X, (float)destination.Y, 0),
            Reward = 500
        };

        _missions[context.Player.UCID] = mission;

        context.GpsService.SetTarget(context.Player,"Taxi pickup",pickup.X,pickup.Y);

        await context.SendMessage(context.Player.UCID,$"^2Taxi uzsakymas gautas. ^7Pickup: ^2{pickup.X:0.0}, {pickup.Y:0.0}",cancellationToken);
    }

    public async Task OnStopAsync(JobContext context, CancellationToken cancellationToken)
    {
        _missions.Remove(context.Player.UCID);
        await context.GpsService.ClearTargetAsync(context.Player, cancellationToken);
        await context.SendMessage(context.Player.UCID,"^1Taxi darbas baigtas.",cancellationToken);
    }

    public async Task OnPlayerMoveAsync(JobContext context,CancellationToken cancellationToken)
    {
        var player = context.Player;

        if (!_missions.TryGetValue(player.UCID, out var mission))
            return;

        switch (mission.State)
        {
            case TaxiState.DrivingToPickup:
                await HandlePickupAsync(context, mission, cancellationToken);
                break;

            case TaxiState.DrivingToDestination:
                await HandleDestinationAsync(context, mission, cancellationToken);
                break;
            case TaxiState.Cooldown:
                await HandleCooldownAsync(context, mission, cancellationToken);
                break;
        }
    }

    private async Task HandlePickupAsync(JobContext context,TaxiMission mission,CancellationToken cancellationToken)
    {
        var player = context.Player;

        if (!IsStoppedAtTarget(player,mission.Pickup.X,mission.Pickup.Y,PickupDistance))
            return;

        mission.State = TaxiState.DrivingToDestination;

        context.CheckpointManager.SetCheckpoint(player,"taxi_destination",mission.Destination.X,mission.Destination.Y,15.0);

        context.GpsService.SetTarget(player,"Taxi destination",mission.Destination.X,mission.Destination.Y);

        await context.SendMessage(player.UCID,"^2Keleivis isedo. Vaziuok i keliones tiksla.",cancellationToken);
    }

    private async Task HandleDestinationAsync(JobContext context,TaxiMission mission,CancellationToken cancellationToken)
    {
        var player = context.Player;

        if (!IsStoppedAtTarget(player,mission.Destination.X,mission.Destination.Y,DestinationDistance))
            return;

        await context.EconomyService.AddMoneyAsync(player, mission.Reward);
        await context.DatabaseService.SavePlayerAsync(player, cancellationToken);

        await context.SendMessage(player.UCID,$"^2Keleivis islipo. Uzdirbai ^3${mission.Reward}.",cancellationToken);

        await StartCooldownAsync(context, mission, cancellationToken);
    }

    private bool IsStoppedAtTarget(Player player,float targetX,float targetY,double maxDistance)
    {
        var dx = player.Vehicle.X - targetX;
        var dy = player.Vehicle.Y - targetY;

        var distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance > maxDistance)
            return false;

        return player.Vehicle.Speed <= MaxStopSpeed;
    }

    private async Task StartNextMissionAsync(JobContext context, CancellationToken cancellationToken)
    {
        var pickup = GetRandomPoint(_points.Pickup);
        var destination = GetRandomPoint(_points.Destination);

        var mission = new TaxiMission
        {
            State = TaxiState.DrivingToPickup,
            Pickup = new System.Numerics.Vector3((float)pickup.X,(float)pickup.Y,0),

            Destination = new System.Numerics.Vector3((float)destination.X,(float)destination.Y,0),

            Reward = 500
        };

        _missions[context.Player.UCID] = mission;

        context.CheckpointManager.SetCheckpoint(context.Player,"taxi_pickup",pickup.X,pickup.Y,15.0);

        context.GpsService.SetTarget(context.Player,"Taxi pickup",pickup.X,pickup.Y);

        await context.SendMessage(context.Player.UCID,"^3Naujas taxi uzsakymas gautas.",cancellationToken);
    }

    private TaxiPoint GetRandomPoint(List<TaxiPoint> points)
    {
        return points[_random.Next(points.Count)];
    }

    private async Task StartCooldownAsync(JobContext context, TaxiMission mission, CancellationToken cancellationToken)
    {
        var cooldownMinutes = _random.Next(MinCooldownMinutes, MaxCooldownMinutes + 1);

        mission.State = TaxiState.Cooldown;
        mission.CooldownUntil = DateTime.UtcNow.AddMinutes(cooldownMinutes);

        await context.GpsService.ClearTargetAsync(context.Player, cancellationToken);
    }

    private async Task HandleCooldownAsync(JobContext context, TaxiMission mission, CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow < mission.CooldownUntil)
            return;

        await StartNextMissionAsync(context, cancellationToken);
    }


}