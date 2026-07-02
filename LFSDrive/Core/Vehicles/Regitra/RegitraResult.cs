namespace LfsCruise.Core.Vehicles.Regitra;

public sealed class RegitraResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static RegitraResult Ok(string message) => new() { Success = true, Message = message };
    public static RegitraResult Fail(string message) => new() { Success = false, Message = message };
}