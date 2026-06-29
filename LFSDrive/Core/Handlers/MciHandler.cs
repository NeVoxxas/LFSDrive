using LfsCruise.Core.Players;
using LfsCruise.Core.Progression;
using LfsCruise.Core.World;
using LfsCruise.InSim.Packets;

namespace LfsCruise.InSim.Handlers;

public sealed class MciHandler
{
    private readonly PlayerManager _playerManager;
    private readonly ZoneManager _zoneManager;
    private readonly ProgressionService _progressionService;

    public MciHandler(
        PlayerManager playerManager,
        ZoneManager zoneManager,
        ProgressionService progressionService)
    {
        _playerManager = playerManager;
        _zoneManager = zoneManager;
        _progressionService = progressionService;
    }

    public void Handle(MciPacket mci)
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
        }
    }
}