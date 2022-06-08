using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SquadTopography.SquadMcMapDataExtractor
{
    internal sealed class JsonMapInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
        [JsonPropertyName("heightmap")]
        public JsonMapInfoHeightMap? HeightMapJson { get; set; } = null;
        [JsonPropertyName("extra")]
        public JsonElement? ExtraJson { get; set; } = null;
        [JsonPropertyName("locations")]
        public JsonElement? LocationsJson { get; set; } = null;

        public JsonMapInfoExtra? GetExtra()
        {
            if (!ExtraJson.HasValue)
            {
                return null;
            }

            var extraJson = ExtraJson.Value;

            var scaleJsonArr = extraJson.GetProperty("scale");
            var scaleX = scaleJsonArr[0].GetSingle();
            var scaleY = scaleJsonArr[1].GetSingle();
            var scaleZ = scaleJsonArr[2].GetSingle();
            var scale = new Vector3(scaleX, scaleY, scaleZ);

            var levelsJsonArr = extraJson.GetProperty("levels");
            var levelsX = levelsJsonArr[0].GetSingle();
            var levelsY = levelsJsonArr[1].GetSingle();
            var levels = new Vector2(levelsX, levelsY);

            var miniMapJsonArr = extraJson.GetProperty("minimap");
            var miniMapCoords = new List<Vector2>();
            foreach (var coordsJsonArr in miniMapJsonArr.EnumerateArray())
            {
                var x = coordsJsonArr[0].GetSingle();
                var y = coordsJsonArr[1].GetSingle();
                miniMapCoords.Add(new Vector2(x, y));
            }

            var hDimJsonArr = extraJson.GetProperty("hDim");
            var hDimX = hDimJsonArr[0].GetSingle();
            var hDimY = hDimJsonArr[1].GetSingle();
            var hDim = new Vector2(hDimX, hDimY);

            var lOriginJsonArr = extraJson.GetProperty("lOrigin");
            var lOriginX = lOriginJsonArr[0].GetSingle();
            var lOriginY = lOriginJsonArr[1].GetSingle();
            var lOrigin = new Vector2(lOriginX, lOriginY);

            return new JsonMapInfoExtra
            {
                Scale = scale,
                Levels = levels,
                MiniMap = miniMapCoords,
                HDim = hDim,
                LOrigin = lOrigin
            };
        }

        public List<JsonMapInfoLocation> GetLocations()
        {
            if (!LocationsJson.HasValue)
                return new List<JsonMapInfoLocation>();

            var locationsJson = LocationsJson.Value;

            var res = new List<JsonMapInfoLocation>();

            foreach (var location in locationsJson.EnumerateArray())
            {
                var name = location[0].GetString();
                if (name == null)
                    throw new Exception("invalid format");

                var x = location[1][0].GetInt32();
                var y = location[1][1].GetInt32();

                res.Add(new JsonMapInfoLocation
                {
                    Name = name,
                    X = x,
                    Y = y
                });
            }

            return res;
        }
    }
}
