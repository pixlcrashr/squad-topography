using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SquadTopography.Heightmaps
{
    public sealed class HeightmapInfo
    {
        [JsonPropertyName(nameof(Name))]
        public string Name { get; set; } = "";
        [JsonPropertyName(nameof(Scale))]
        public float Scale { get; set; } = 1f;
    }
}
