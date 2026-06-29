namespace LfsCruise.Core.World;

public abstract class Zone
{
    protected Zone(ZoneDto dto)
    {
        Dto = dto;
    }

    public ZoneDto Dto { get; }

    public string Name => Dto.Name;
    public ZoneType Type => Dto.Type;

    public abstract bool Contains(Players.Player player);
}