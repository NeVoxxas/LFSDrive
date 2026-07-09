using System.Text.Json;

namespace LfsCruise.Core.Jobs.Police;

public sealed class PoliceConfigStorage
{
    private const string FilePath = "Data/police.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public PoliceConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            Console.WriteLine($"Police config not found, using defaults: {Path.GetFullPath(FilePath)}");
            return new PoliceConfig();
        }

        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<PoliceConfig>(json, JsonOptions) ?? new PoliceConfig();
    }

    public void Save(PoliceConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        var json = JsonSerializer.Serialize(config, JsonOptions);

        File.WriteAllText(FilePath, json);
    }
}