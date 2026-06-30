using System.Text.Json;

namespace LfsCruise.Core.Jobs.Taxi;

public sealed class TaxiPointStorage
{
    private const string FilePath = "Data/taxi_points.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public TaxiPointConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            Console.WriteLine($"Taxi points config not found: {Path.GetFullPath(FilePath)}");
            return new TaxiPointConfig();
        }

        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<TaxiPointConfig>(
            json,
            JsonOptions) ?? new TaxiPointConfig();
    }

    public void Save(TaxiPointConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        var json = JsonSerializer.Serialize(config, JsonOptions);

        File.WriteAllText(FilePath, json);
    }
}