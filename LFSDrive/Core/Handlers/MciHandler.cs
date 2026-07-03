using LfsCruise.Core.Economy.Bank;
using LfsCruise.Core.GPS;
using LfsCruise.Core.Jobs;
using LfsCruise.Core.Players;
using LfsCruise.Core.Progression;
using LfsCruise.Core.Vehicles.Regitra;
using LfsCruise.Core.World;
using LfsCruise.InSim.Packets;

namespace LfsCruise.InSim.Handlers;

public sealed class MciHandler
{
    private readonly PlayerManager _playerManager;
    private readonly ZoneManager _zoneManager;
    private readonly ProgressionService _progressionService;
    private readonly GpsService _gpsService;
    private readonly JobManager _jobManager;
    private readonly BankZoneService _bankZoneService;
    private readonly RegitraZoneService _regitraZoneService;

    public MciHandler(
        PlayerManager playerManager,
        ZoneManager zoneManager,
        ProgressionService progressionService,
        GpsService gpsService,
        JobManager jobManager,
        BankZoneService bankZoneService,
        RegitraZoneService regitraZoneService)
    {
        _playerManager = playerManager;
        _zoneManager = zoneManager;
        _progressionService = progressionService;
        _gpsService = gpsService;
        _jobManager = jobManager;
        _bankZoneService = bankZoneService;
        _regitraZoneService = regitraZoneService;
    }

    public async Task HandleAsync(MciPacket mci, CancellationToken cancellationToken)
    {
        foreach (var car in mci.Cars)
        {
            var player = _playerManager.GetByPlid(car.PLID);

            if (player is null)
                continue;

            var oldX = player.Vehicle.X;
            var oldY = player.Vehicle.Y;

            var newX = car.XMetres;
            var newY = car.YMetres;

            if (oldX != 0 || oldY != 0)
            {
                var dx = newX - oldX;
                var dy = newY - oldY;
                var distanceMeters = Math.Sqrt(dx * dx + dy * dy);

                if (distanceMeters < 100)
                {
                    _progressionService.AddDistance(
                        player,
                        distanceMeters / 1000.0);
                }
            }

            player.Vehicle.PLID = car.PLID;
            player.Vehicle.Node = car.Node;
            player.Vehicle.X = newX;
            player.Vehicle.Y = newY;
            player.Vehicle.Z = car.ZMetres;
            player.Vehicle.Speed = car.SpeedKmh;
            player.Vehicle.Heading = car.Heading;

            _zoneManager.Update(player);
            await _bankZoneService.OnPlayerMoveAsync(player, cancellationToken);
            await _regitraZoneService.OnPlayerMoveAsync(player, cancellationToken);
            await _jobManager.OnPlayerMoveAsync(player, cancellationToken);
            await _gpsService.UpdateAsync(player, cancellationToken);
        }
    }
}