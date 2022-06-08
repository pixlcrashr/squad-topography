using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using SquadTopography.Heightmaps;

namespace SquadTopography.SquadMcMapDataExtractor
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentOutOfRangeException("Only two input files are required.");
            }

            var squadMcMapDataFilepath = Path.GetFullPath(args[0]);
            var heightmapsOutputDirectory = Path.GetFullPath(args[1]);

            using var squadMcMapDataFile = File.Open(squadMcMapDataFilepath, FileMode.Open, FileAccess.Read);
            var squadMcMapDataInfos = JsonSerializer.Deserialize<List<JsonMapInfo>>(squadMcMapDataFile);

            if (squadMcMapDataInfos == null)
            {
                Console.WriteLine("No map data info found.");
                return;
            }

            squadMcMapDataInfos.ForEach(
                squadMcMapInfo =>
                {
                    var filename = ExtractMapName(squadMcMapInfo);
                    var name = squadMcMapInfo.Name;
                    var scale = squadMcMapInfo.HeightMapJson?.Scale ?? 1;

                    if (filename == null)
                    {
                        Console.WriteLine($"Map {squadMcMapInfo.Name} does not have a heightmap.");
                        return;
                    }

                    var heightmapFilepath = Path.Combine(heightmapsOutputDirectory, $"{filename}.jpg");
                    if (!File.Exists(heightmapFilepath))
                    {
                        Console.WriteLine($"Map {squadMcMapInfo.Name} does not have a heightmap.");
                        return;
                    }

                    var outputFilepath = Path.Combine(heightmapsOutputDirectory, $"{filename}.json");
                    using var outputFile = File.OpenWrite(outputFilepath);
                    JsonSerializer.Serialize(
                        outputFile, new HeightmapInfo
                        {
                            Name = name,
                            Scale = scale
                        }
                    );
                });
        }

        private static string? ExtractMapName(
            JsonMapInfo jsonMapInfo
        )
        {
            var squadMcFilepath = jsonMapInfo.HeightMapJson?.Url;
            return squadMcFilepath == null ? null : Path.GetFileNameWithoutExtension(squadMcFilepath);
        }
    }
}