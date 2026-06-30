namespace LfsCruise.Core.Vehicles.Mods;

public sealed class ModNameConfig
{
    public Dictionary<string, string> Names { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}