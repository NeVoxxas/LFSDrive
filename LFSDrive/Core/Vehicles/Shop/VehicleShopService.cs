namespace LfsCruise.Core.Vehicles.Shop;

public sealed class VehicleShopService
{
    private readonly VehicleShopConfig _config;

    public VehicleShopService(VehicleShopStorage storage)
    {
        _config = storage.Load();

        Console.WriteLine($"Vehicle shop loaded: {_config.Categories.Count} categories");
    }

    public IReadOnlyList<VehicleShopCategory> GetCategories()
    {
        return _config.Categories;
    }

    public VehicleShopCategory? GetCategory(string categoryId)
    {
        return _config.Categories.FirstOrDefault(c =>
            c.Id.Equals(categoryId, StringComparison.OrdinalIgnoreCase));
    }

    public VehicleShopItem? GetVehicle(string carCode)
    {
        return _config.Categories
            .SelectMany(c => c.Vehicles)
            .FirstOrDefault(v =>
                v.CarCode.Equals(carCode, StringComparison.OrdinalIgnoreCase));
    }

    public VehicleShopItem? GetVehicleByCode(string carCode)
    {
        foreach (var category in _config.Categories)
        {
            var vehicle = category.Vehicles
                .FirstOrDefault(v => v.CarCode == carCode);

            if (vehicle != null)
                return vehicle;
        }

        return null;
    }
}