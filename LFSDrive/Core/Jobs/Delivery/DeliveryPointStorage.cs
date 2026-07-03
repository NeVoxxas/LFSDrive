using System.Text.Json;

namespace LfsCruise.Core.Jobs.Delivery;

public sealed class DeliveryPointStorage
{
    private const string FilePath = "Data/delivery.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public DeliveryConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            Console.WriteLine($"Delivery config not found: {Path.GetFullPath(FilePath)}");
            return new DeliveryConfig();
        }

        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<DeliveryConfig>(json, JsonOptions) ?? new DeliveryConfig();
    }

    public void Save(DeliveryConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        var json = JsonSerializer.Serialize(config, JsonOptions);

        File.WriteAllText(FilePath, json);
    }
}