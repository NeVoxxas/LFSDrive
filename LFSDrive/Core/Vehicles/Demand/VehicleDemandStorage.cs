using System.Text.Json;

namespace LfsCruise.Core.Vehicles.Demand;

public sealed class VehicleDemandStorage
{
    private const string FilePath = "Data/vehicle_demand.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public VehicleDemandConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            Console.WriteLine($"Vehicle demand config not found, starting fresh: {Path.GetFullPath(FilePath)}");
            return new VehicleDemandConfig();
        }

        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<VehicleDemandConfig>(json, JsonOptions) ?? new VehicleDemandConfig();
    }

    public void Save(VehicleDemandConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        var json = JsonSerializer.Serialize(config, JsonOptions);

        File.WriteAllText(FilePath, json);
    }
}
