using System.Text.Json;

namespace LfsCruise.Core.Vehicles.Shop;

public sealed class VehicleShopStorage
{
    private const string FilePath = "Data/vehicle_shop.json";

    public VehicleShopConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            Console.WriteLine($"Vehicle shop config not found: {Path.GetFullPath(FilePath)}");
            return new VehicleShopConfig();
        }

        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<VehicleShopConfig>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new VehicleShopConfig();
    }
}