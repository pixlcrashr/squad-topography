using System.Text.Json.Serialization;

namespace SquadTopography.SquadMcMapDataExtractor;

internal sealed class JsonMapInfoHeightMap
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";
    [JsonPropertyName("tile")]
    public string Tile { get; set; } = "";
    [JsonPropertyName("scale")]
    public float Scale { get; set; } = 0;
}