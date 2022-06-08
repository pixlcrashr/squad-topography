using System.Data;
using System.Net.Mime;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SquadTopography.Heightmaps
{
    public sealed class HeightmapBuilder
    {
        private readonly string _filename;
        private readonly float _scale;
        private readonly int _size;

        public static HeightmapBuilder FromFiles(
            string heightmapsFolder,
            string name,
            int size
        )
        {
            var heightmapFilepath = Path.GetFullPath(Path.Combine(heightmapsFolder, $"{name}.jpg"));
            var heightmapInfoFilepath = Path.GetFullPath(Path.Combine(heightmapsFolder, $"{name}.json"));

            using var infoFile = File.Open(heightmapInfoFilepath, FileMode.Open, FileAccess.Read);
            var info = JsonSerializer.Deserialize<HeightmapInfo>(infoFile);

            if (info == null)
            {
                throw new Exception("Heightmap info has invalid format.");
            }

            return new HeightmapBuilder(heightmapFilepath, info.Scale, size);
        }

        public HeightmapBuilder(string filename, float scale, int size = 0)
        {
            _filename = filename;
            _scale = scale;
            _size = size;

            if (_size > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "should be of ushort max size");
            }
        }

        public Heightmap Build()
        {
            using var image = Image.Load<Rgb24>(_filename);

            if (_size > 0)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(_size),
                    Mode = ResizeMode.Max
                }));
            }

            var heightmap = new Heightmap(
                (ushort)_size,
                (ushort)_size
            );

            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var pixel = image[x, y];
                    heightmap.SetHeight(
                        x,
                        y,
                        GetBlueRedHeight(pixel.R, pixel.B, _scale)
                    );
                }
            }

            return heightmap;
        }

        private static float GetBlueRedHeight(
            byte r,
            byte b,
            float scale)
        {
            return (255 - r + b) * scale;
        }
    }
}
