using System.Text.Json;

namespace LfsCruise.Core.Vehicles.Tow;

public sealed class TowConfigStorage
{
    private const string FilePath = "Data/tow.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public TowConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            Console.WriteLine($"Tow config not found, using defaults: {Path.GetFullPath(FilePath)}");
            return new TowConfig();
        }

        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<TowConfig>(json, JsonOptions) ?? new TowConfig();
    }

    public void Save(TowConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        var json = JsonSerializer.Serialize(config, JsonOptions);

        File.WriteAllText(FilePath, json);
    }
}