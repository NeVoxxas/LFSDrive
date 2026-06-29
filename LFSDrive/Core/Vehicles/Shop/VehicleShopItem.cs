namespace LfsCruise.Core.Vehicles.Shop;

public sealed class VehicleShopItem
{
    public string CarCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Price { get; set; }
}