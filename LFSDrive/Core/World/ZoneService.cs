namespace LfsCruise.Core.World;

public sealed class ZoneService
{
    private readonly ZoneManager _zoneManager;
    private readonly ZoneStorage _zoneStorage;
    private readonly List<ZoneDto> _zones = new();

    public ZoneService(ZoneManager zoneManager, ZoneStorage zoneStorage)
    {
        _zoneManager = zoneManager;
        _zoneStorage = zoneStorage;
    }

    public void Load()
    {
        _zoneManager.Clear();
        _zones.Clear();

        var zones = _zoneStorage.Load();

        foreach (var zone in zones)
        {
            AddLoadedZone(zone);
        }

        Console.WriteLine($"Zones loaded: {_zones.Count}");
    }

    public ZoneDto CreateZone(ZoneType type, string name, double x, double y, double radius)
    {
        var dto = new ZoneDto
        {
            Type = type,
            Name = name,
            X = x,
            Y = y,
            Radius = radius
        };

        _zones.Add(dto);
        _zoneManager.Add(new CircleZone(dto));

        return dto;
    }

    public bool RemoveZone(string name)
    {
        var dto = _zones.FirstOrDefault(z =>
            z.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (dto is null)
            return false;

        _zones.Remove(dto);

        LoadFromMemory();

        return true;
    }

    private void LoadFromMemory()
    {
        _zoneManager.Clear();

        foreach (var zone in _zones)
        {
            _zoneManager.Add(new CircleZone(zone));
        }
    }

    public void Save()
    {
        _zoneStorage.Save(_zones);
    }

    public void Reload()
    {
        Load();
    }

    public IReadOnlyList<ZoneDto> GetAll()
    {
        return _zones;
    }

    private void AddLoadedZone(ZoneDto dto)
    {
        _zones.Add(dto);

        _zoneManager.Add(new CircleZone(dto));
    }
}