namespace LfsCruise.Core.Vehicles.Garage;

public sealed class GarageResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static GarageResult Ok(string message) => new() { Success = true, Message = message };
    public static GarageResult Fail(string message) => new() { Success = false, Message = message };
}