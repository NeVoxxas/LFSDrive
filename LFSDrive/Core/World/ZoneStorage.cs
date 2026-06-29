using System.Text.Json;
using System.Text.Json.Serialization;
namespace LfsCruise.Core.World;

public sealed class ZoneStorage
{
    private const string FileName = "Data/zones.json";

    public List<ZoneDto> Load()
    {
        if (!File.Exists(FileName))
            return new();

        var json = File.ReadAllText(FileName);

        return JsonSerializer.Deserialize<List<ZoneDto>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
            new JsonStringEnumConverter()
                }
            })
            ?? new();
    }

    public void Save(List<ZoneDto> zones)
    {
        var json = JsonSerializer.Serialize(
            zones,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                new JsonStringEnumConverter()
                }
            });

        Directory.CreateDirectory(Path.GetDirectoryName(FileName)!);
        File.WriteAllText(FileName, json);
    }
}