namespace LfsCruise.Core.Jobs.Police;

public sealed class PoliceFineResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static PoliceFineResult Ok(string message) => new() { Success = true, Message = message };
    public static PoliceFineResult Fail(string message) => new() { Success = false, Message = message };
}