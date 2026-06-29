namespace LfsCruise.Core.World;

public static class WorldLoader
{
    public static void LoadZones(ZoneManager zoneManager)
    {
        var storage = new ZoneStorage();
        var zones = storage.Load();

        Console.WriteLine($"Loaded zones: {zones.Count}");

        foreach (var zone in zones)
        {
            Console.WriteLine($"Zone loaded: {zone.Name} ({zone.X}, {zone.Y}) R:{zone.Radius}");

            switch (zone.Type)
            {
                case ZoneType.Test:
                    zoneManager.Add(new CircleZone(zone));
                    break;

                case ZoneType.FuelStation:
                    zoneManager.Add(new CircleZone(zone));
                    break;

                case ZoneType.Garage:
                    zoneManager.Add(new CircleZone(zone));
                    break;

                default:
                    Console.WriteLine($"Unknown zone type: {zone.Type}");
                    break;
            }
        }

    }
}