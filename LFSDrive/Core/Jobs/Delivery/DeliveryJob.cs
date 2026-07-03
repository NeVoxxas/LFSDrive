using LfsCruise.Core.Players;
using LfsCruise.Core.Vehicles;

namespace LfsCruise.Core.Jobs.Delivery;

public sealed class DeliveryJob : IJob
{
    private const double MaxStopSpeed = 1.0;

    private readonly DeliveryConfig _config;
    private readonly VehicleOwnershipService _ownershipService;
    private readonly JobService _jobService;
    private readonly Random _random = new();

    private readonly Dictionary<byte, DeliveryMission> _missions = new();

    public DeliveryJob(
        DeliveryPointStorage storage,
        VehicleOwnershipService ownershipService,
        JobService jobService)
    {
        _config = storage.Load();
        _ownershipService = ownershipService;
        _jobService = jobService;

        if (!string.IsNullOrWhiteSpace(_config.CarCode))
        {
            _jobService.RegisterJobVehicle(_config.CarCode, JobType.Delivery);
        }
    }

    public JobType Type => JobType.Delivery;
    public string Name => "DPD";

    public async Task OnStartAsync(JobContext context, CancellationToken cancellationToken)
    {
        var player = context.Player;

        if (_config.Clients.Count == 0 || string.IsNullOrWhiteSpace(_config.CarCode))
        {
            await context.SendMessage(player.UCID, "^1Kurjerio darbas nesukonfiguruotas.", cancellationToken);
            return;
        }

        var alreadyOwns = await _ownershipService.OwnsVehicleAsync(player, _config.CarCode, cancellationToken);

        if (!alreadyOwns)
        {
            await _ownershipService.AddVehicleAsync(player, _config.CarCode, cancellationToken);
            await context.SendMessage(
                player.UCID,
                "^2Gavai kurjerio automobili i garaza. ^7Nuvaziuok i boksus ir persesk i ji.",
                cancellationToken);
        }

        StartNewCycle(context);

        await context.SendMessage(player.UCID, "^2Kurjerio darbas pradetas. ^7Vaziuok i sandeli.", cancellationToken);
    }

    public async Task OnStopAsync(JobContext context, CancellationToken cancellationToken)
    {
        _missions.Remove(context.Player.UCID);
        await context.GpsService.ClearTargetAsync(context.Player, cancellationToken);
        context.JobService.ClearStatus(context.Player); // NAUJA
        await context.SendMessage(context.Player.UCID, "^1Kurjerio darbas baigtas.", cancellationToken);
    }

    public async Task OnPlayerMoveAsync(JobContext context, CancellationToken cancellationToken)
    {
        var player = context.Player;

        if (!_missions.TryGetValue(player.UCID, out var mission))
            return;

        switch (mission.State)
        {
            case DeliveryState.DrivingToHqInitial:
                await HandleHqLoadAsync(context, mission, cancellationToken);
                break;

            case DeliveryState.DrivingToClient:
                await HandleClientDeliveryAsync(context, mission, cancellationToken);
                break;

            case DeliveryState.DrivingToPickupReverse:
                await HandleReversePickupAsync(context, mission, cancellationToken);
                break;

            case DeliveryState.DrivingToHqReturn:
                await HandleHqReturnAsync(context, mission, cancellationToken);
                break;

            case DeliveryState.Cooldown:
                await HandleCooldownAsync(context, mission, cancellationToken);
                break;
        }
    }

    private void StartNewCycle(JobContext context)
    {
        var mission = new DeliveryMission
        {
            State = DeliveryState.DrivingToHqInitial,
            RemainingDeliveries = 5,
            Origin = _config.Hq,
            Target = _config.Hq
        };

        _missions[context.Player.UCID] = mission;

        context.GpsService.SetTarget(context.Player, "Sandelis", _config.Hq.X, _config.Hq.Y);
        context.JobService.SetStatus(context.Player, "^7DPD: -> Sandelis"); // NAUJA
    }

    private async Task HandleHqLoadAsync(JobContext context, DeliveryMission mission, CancellationToken cancellationToken)
    {
        if (!IsStoppedAtTarget(context.Player, mission.Target))
            return;

        var client = GetRandomClient();

        mission.State = DeliveryState.DrivingToClient;
        mission.Origin = _config.Hq;
        mission.Target = client;

        context.GpsService.SetTarget(context.Player, client.Name, client.X, client.Y);
        context.JobService.SetStatus(context.Player, $"^7DPD: {mission.RemainingDeliveries}/5"); // NAUJA

        await context.SendMessage(
            context.Player.UCID,
            $"^2Pakrovei 5 siuntas. ^7Pirmas adresas: {client.Name} ({mission.RemainingDeliveries} liko)",
            cancellationToken);
    }

    private async Task HandleClientDeliveryAsync(JobContext context, DeliveryMission mission, CancellationToken cancellationToken)
    {
        if (!IsStoppedAtTarget(context.Player, mission.Target))
            return;

        var reward = CalculateReward(mission.Origin, mission.Target);

        await context.EconomyService.AddMoneyAsync(context.Player, reward);
        await context.DatabaseService.SavePlayerAsync(context.Player, cancellationToken);

        mission.RemainingDeliveries--;

        if (mission.RemainingDeliveries > 0)
        {
            var next = GetRandomClient();

            mission.Origin = mission.Target;
            mission.Target = next;

            context.GpsService.SetTarget(context.Player, next.Name, next.X, next.Y);
            context.JobService.SetStatus(context.Player, $"^7DPD: {mission.RemainingDeliveries}/5"); // NAUJA

            await context.SendMessage(
                context.Player.UCID,
                $"^2Pristatyta! Uzdirbai ^3${reward}. ^7Sekantis: {next.Name} ({mission.RemainingDeliveries} liko)",
                cancellationToken);

            return;
        }

        // Visos 5 pristatytos
        if (_random.NextDouble() < _config.ReverseChance)
        {
            var pickup = GetRandomClient();

            mission.State = DeliveryState.DrivingToPickupReverse;
            mission.Origin = mission.Target;
            mission.Target = pickup;

            context.GpsService.SetTarget(context.Player, pickup.Name, pickup.X, pickup.Y);
            context.JobService.SetStatus(context.Player, "^3DPD: pasiimt siunta"); // NAUJA

            await context.SendMessage(
                context.Player.UCID,
                $"^2Paskutine siunta pristatyta (^3${reward}^2). ^3Papildoma uzduotis: ^7pasiimk siunta pas {pickup.Name} ir parvezk i sandeli.",
                cancellationToken);

            return;
        }

        await context.SendMessage(
            context.Player.UCID,
            $"^2Visos siuntos pristatytos! Uzdirbai ^3${reward} ^2uz paskutine. ^7Kita partija netrukus.",
            cancellationToken);

        await StartCooldownAsync(context, mission, cancellationToken);
    }

    private async Task HandleReversePickupAsync(JobContext context, DeliveryMission mission, CancellationToken cancellationToken)
    {
        if (!IsStoppedAtTarget(context.Player, mission.Target))
            return;

        mission.State = DeliveryState.DrivingToHqReturn;
        mission.Origin = mission.Target;
        mission.Target = _config.Hq;

        context.GpsService.SetTarget(context.Player, "Sandelis", _config.Hq.X, _config.Hq.Y);
        context.JobService.SetStatus(context.Player, "^3DPD: grazinam i sandeli"); // NAUJA

        await context.SendMessage(context.Player.UCID, "^2Siunta pasiimta. ^7Grazink i sandeli.", cancellationToken);
    }

    private async Task HandleHqReturnAsync(JobContext context, DeliveryMission mission, CancellationToken cancellationToken)
    {
        if (!IsStoppedAtTarget(context.Player, mission.Target))
            return;

        var reward = CalculateReward(mission.Origin, mission.Target);

        await context.EconomyService.AddMoneyAsync(context.Player, reward);
        await context.DatabaseService.SavePlayerAsync(context.Player, cancellationToken);

        await context.SendMessage(
            context.Player.UCID,
            $"^2Siunta grazinta i sandeli. Uzdirbai ^3${reward}. ^7Kita partija netrukus.",
            cancellationToken);

        await StartCooldownAsync(context, mission, cancellationToken);
    }

    private async Task HandleCooldownAsync(JobContext context, DeliveryMission mission, CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow < mission.CooldownUntil)
            return;

        StartNewCycle(context);

        await context.SendMessage(context.Player.UCID, "^3Nauja siuntu partija laukia sandelyje.", cancellationToken);
    }

    private async Task StartCooldownAsync(JobContext context, DeliveryMission mission, CancellationToken cancellationToken)
    {
        var cooldownMinutes = _random.Next(_config.MinCooldownMinutes, _config.MaxCooldownMinutes + 1);

        mission.State = DeliveryState.Cooldown;
        mission.CooldownUntil = DateTime.UtcNow.AddMinutes(cooldownMinutes);

        await context.GpsService.ClearTargetAsync(context.Player, cancellationToken);
        context.JobService.SetStatus(context.Player, "^1DPD: laukiam"); // NAUJA
    }

    private int CalculateReward(DeliveryPoint from, DeliveryPoint to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        var distanceKm = Math.Sqrt(dx * dx + dy * dy) / 1000.0;

        var reward = _config.Fare.BaseFare + (int)Math.Round(distanceKm * _config.Fare.PricePerKm);

        return Math.Clamp(reward, _config.Fare.MinReward, _config.Fare.MaxReward);
    }

    private bool IsStoppedAtTarget(Player player, DeliveryPoint target)
    {
        var dx = player.Vehicle.X - target.X;
        var dy = player.Vehicle.Y - target.Y;

        var distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance > _config.ArrivalRadius)
            return false;

        return player.Vehicle.Speed <= MaxStopSpeed;
    }

    private DeliveryPoint GetRandomClient()
    {
        return _config.Clients[_random.Next(_config.Clients.Count)];
    }
}