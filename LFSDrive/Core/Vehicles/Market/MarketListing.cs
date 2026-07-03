namespace LfsCruise.Core.Vehicles.Market;

public sealed class MarketListing
{
    public int Id { get; init; }

    public int SellerPlayerId { get; init; }

    public string SellerUsername { get; init; } = string.Empty;

    public string CarCode { get; init; } = string.Empty;

    public string DisplayName {  get; init; } = string.Empty;

    public string CategoryId {  get; init; } = string.Empty;

    public int Price { get; init; }

    public DateTime CreatedAt { get; init; }
}
