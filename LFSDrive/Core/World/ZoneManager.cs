using LfsCruise.Core.Players;

namespace LfsCruise.Core.World;

public sealed class ZoneManager
{
    private readonly List<Zone> _zones = new();
    private readonly Dictionary<byte, HashSet<string>> _playerZones = new();
    public IReadOnlyList<Zone> Zones => _zones;

    public void Add(Zone zone)
    {
        _zones.Add(zone);
    }

    public List<ZoneDto> ToDtoList()
    {
        return _zones.Select(z => z.Dto).ToList();
    }

    public void Update(Player player)
    {
        //Console.WriteLine($"Zone update called. Zones count: {_zones.Count}");
        if (!_playerZones.TryGetValue(player.UCID, out var currentZones))
        {
            currentZones = new HashSet<string>();
            _playerZones[player.UCID] = currentZones;
        }

        foreach (var zone in _zones)
        {
            var inside = zone.Contains(player);
            var wasInside = currentZones.Contains(zone.Name);

            if (inside && !wasInside)
            {
                currentZones.Add(zone.Name);
                Console.WriteLine($"{player.Username} entered zone: {zone.Name}");
            }
            else if (!inside && wasInside)
            {
                currentZones.Remove(zone.Name);
                Console.WriteLine($"{player.Username} left zone: {zone.Name}");
            }
        }

    }

    public void RemovePlayer(byte ucid)
    {
        _playerZones.Remove(ucid);
    }

    public void Clear()
    {
        _zones.Clear();
        _playerZones.Clear();
    }
}