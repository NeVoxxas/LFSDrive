using System.Text.Json.Serialization;

namespace LfsCruise.Core.World;

public sealed class ZoneDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public ZoneType Type { get; set; } = ZoneType.Test;

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("radius")]
    public double Radius { get; set; }
}