namespace LfsCruise.Core.Vehicles.Market;

public sealed class MarketResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static MarketResult Ok(string message) => new() { Success = true, Message = message };
    public static MarketResult Fail(string message) => new() { Success = false, Message = message };
}
