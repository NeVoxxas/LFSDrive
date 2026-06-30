using System.Text.Json;

namespace LfsCruise.Core.Vehicles.Mods;

public sealed class ModNameStorage
{
    private const string FilePath = "Data/mod_names.json";

    public ModNameConfig Load()
    {
        if (!File.Exists(FilePath))
            return new ModNameConfig();

        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<ModNameConfig>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ModNameConfig();
    }

    public void Save(ModNameConfig config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, json);
    }
}