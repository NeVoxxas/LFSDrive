using System.Text.Json;

namespace LfsCruise.Core.Vehicles.Starter;

public sealed class StarterCarStorage
{
    private const string FilePath = "Data/starter_cars.json";

    public StarterCarConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            Console.WriteLine($"Starter cars config not found: {Path.GetFullPath(FilePath)}");
            return new StarterCarConfig();
        }

        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<StarterCarConfig>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new StarterCarConfig();
    }
}