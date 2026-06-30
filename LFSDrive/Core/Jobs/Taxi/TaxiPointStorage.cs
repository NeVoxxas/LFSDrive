using System.Text.Json;

namespace LfsCruise.Core.Jobs.Taxi;

public sealed class TaxiPointStorage
{
    private const string FilePath = "Data/taxi_points.json";

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
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new TaxiPointConfig();
    }
}