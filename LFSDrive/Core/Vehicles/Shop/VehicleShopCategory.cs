namespace LfsCruise.Core.Vehicles.Shop;

public sealed class VehicleShopCategory
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double RequiredLicense { get; set; }
    public List<VehicleShopItem> Vehicles { get; set; } = new();
}