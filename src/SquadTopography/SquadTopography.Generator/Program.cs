using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SquadTopography.Heightmaps;

namespace SquadTopography.Generator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(args));

            var heightmapsFolder = Path.GetFullPath(args[0]);
            var outputFolder = Path.GetFullPath(args[1]);

            var files = Directory.GetFiles(heightmapsFolder).Select(Path.GetFileNameWithoutExtension).Distinct().ToList();

            files.ForEach(name => GenerateHeightmap(heightmapsFolder, name ?? "", outputFolder));
        }

        public static void GenerateHeightmap(string heightmapsFolder, string name, string outputFolder)
        {
            Console.WriteLine($"Generating heightmap for '{name}'");
            using var heightmap = HeightmapBuilder.FromFiles(heightmapsFolder, name, 8000).Build();

            using var outputImage = new Image<Rgb24>(8000, 8000, new Rgb24(255, 255, 255));

            for (var x = 0; x < outputImage.Width; x++)
            {
                for (var y = 0; y < outputImage.Height; y++)
                {
                    var currentLayerIndex = heightmap.GetHeightLayerIndex(x, y);
                    var topLayerIndex = heightmap.GetHeightLayerIndex(x, y + 1) ?? 0;
                    var bottomLayerIndex = heightmap.GetHeightLayerIndex(x, y - 1) ?? 0;
                    var leftLayerIndex = heightmap.GetHeightLayerIndex(x - 1, y) ?? 0;
                    var rightLayerIndex = heightmap.GetHeightLayerIndex(x + 1, y) ?? 0;

                    var nearbyMaxLayerIndex = Max(topLayerIndex, bottomLayerIndex, leftLayerIndex, rightLayerIndex);
                    if (nearbyMaxLayerIndex <= currentLayerIndex)
                        continue;

                    if (currentLayerIndex % 10 == 0)
                    {
                        outputImage.Mutate(context => context.Draw(
                            new Pen(Color.Black, 2f),
                            new RectangleF(x, y, 1, 1)));
                    }
                    else
                    {
                        outputImage[x, y] = new Rgb24(0, 0, 0);
                    }
                }
            }

            outputImage.Save(Path.Combine(outputFolder, $"{name}.heightmap.jpg"), new JpegEncoder());
            Console.WriteLine($"Generated heightmap for '{name}'");
        }

        private static int Max(
            params int[] values
        )
        {
            var highest = 0;

            foreach (var value in values)
            {
                if (value > highest)
                    highest = value;
            }

            return highest;
        }

        /*private static void ProcessMapInfo(
            JsonMapInfo mapInfo
        )
        {
            var heightMap = mapInfo.HeightMapJson;
            if (heightMap == null)
            {
                throw new Exception("invalid height map");
            }

            var localPath = $".{heightMap.Url}";

            Console.WriteLine($"Processing map image: {localPath}");

            try
            {
                using var image = Image.Load<Rgb24>(localPath);

                var topographyHeightMap = GetTopographyHeightMap(image, heightMap, 2);

                using var outputImage = new Image<Rgb24>(image.Width, image.Height, new Rgb24(255, 255, 255));

                for (var x = 0; x < image.Width; x++)
                {
                    for (var y = 0; y < image.Height; y++)
                    {
                        var currentHeight = topographyHeightMap[x, y];
                        var top = GetTopographyHeight(topographyHeightMap, x, y + 1) ?? 0;
                        var bottom = GetTopographyHeight(topographyHeightMap, x, y - 1) ?? 0;
                        var left = GetTopographyHeight(topographyHeightMap, x - 1, y) ?? 0;
                        var right = GetTopographyHeight(topographyHeightMap, x + 1, y) ?? 0;

                        var nearbyMaxHeight = Max(top, bottom, left, right);
                        if (nearbyMaxHeight <= currentHeight)
                            continue;

                        if (nearbyMaxHeight % 10 == 0)
                        {
                            outputImage.Mutate(context => context.Draw(
                                new Pen(Color.Black, 2f),
                                new RectangleF(x, y, 1, 1)));
                        }
                        else
                        {
                            outputImage[x, y] = new Rgb24(0, 0, 0);
                        }
                    }
                }

                outputImage.Save($".{heightMap.Url}.edited.jpg", new JpegEncoder());
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Map image {localPath} not found. Continuing...");
                return;
            }
        }

        private static int Max(
            params int[] values
        )
        {
            var highest = 0;

            foreach (var value in values)
            {
                if (value > highest)
                    highest = value;
            }

            return highest;
        }

        private static int? GetTopographyHeight(
            int[,] map,
            int x,
            int y
        )
        {
            var dim1Length = map.GetLength(0);
            var dim2Length = map.GetLength(1);
            if (x >= dim1Length || y >= dim2Length || x < 0 || y < 0)
            {
                return null;
            }

            return map[x, y];
        }

        private static int[,] GetTopographyHeightMap(Image<Rgb24> image, JsonMapInfoHeightMap heightMap, int step)
        {
            var topographyHeightMap = new int[image.Width, image.Height];

            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var normalizedHeight = GetNormalizedHeight(heightMap.Scale, image[x, y]);

                    normalizedHeight = Convert.ToInt32(Math.Round(normalizedHeight / (float)step) * step);

                    topographyHeightMap[x, y] = normalizedHeight;
                }
            }

            return topographyHeightMap;
        }

        private static int GetNormalizedHeight(
            float scale,
            Rgb24 pixel
        )
        {
            var height = GetHeight(scale, pixel);

            return Convert.ToInt32(Math.Round(height));
        }

        private static Rgb24 GetHeightColor(
            int height,
            int maxHeight
        )
        {
            var b = Convert.ToByte(
                (height / (maxHeight * 1f)) * 255
            );

            return new Rgb24(b, b, b);
        }

        private static float GetMaxHeight(
            Image<Rgb24> image,
            float scale
        )
        {
            var maxHeight = 0f;
            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var height = GetHeight(scale, image[x, y]);
                    if (height > maxHeight)
                        maxHeight = height;
                }
            }

            return maxHeight;
        }

        public static float GetHeight(
            float scale,
            Rgb24 pixel
        )
        {
            return (255 - pixel.R + pixel.B) * scale;
        }*/
    }
}