using System.Text.Json;

namespace LfsCruise.Core.Vehicles.Regitra;

public sealed class RegitraConfigStorage
{
    private const string FilePath = "Data/regitra.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public RegitraConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            Console.WriteLine($"Regitra config not found: {Path.GetFullPath(FilePath)}");
            return new RegitraConfig();
        }

        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<RegitraConfig>(json, JsonOptions) ?? new RegitraConfig();
    }

    public void Save(RegitraConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        var json = JsonSerializer.Serialize(config, JsonOptions);

        File.WriteAllText(FilePath, json);
    }
}