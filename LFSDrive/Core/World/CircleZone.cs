using LfsCruise.Core.Players;

namespace LfsCruise.Core.World;

public sealed class CircleZone : Zone
{
    public CircleZone(ZoneDto dto)
        : base(dto)
    {
    }

    public override bool Contains(Player player)
    {
        var dx = player.Vehicle.X - Dto.X;
        var dy = player.Vehicle.Y - Dto.Y;

        return dx * dx + dy * dy <= Dto.Radius * Dto.Radius;
    }
}