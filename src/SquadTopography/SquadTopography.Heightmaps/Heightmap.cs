
namespace SquadTopography.Heightmaps
{
    public sealed class Heightmap : IDisposable
    {
        public ushort Width { get; }
        public ushort Height { get; }

        public Heightmap(
            ushort width = ushort.MaxValue,
            ushort height = ushort.MaxValue)
        {
            _heightmap = new float[width, height];
            Width = width;
            Height = height;
        }

        private readonly float[,] _heightmap;

        public void SetHeight(
            int x,
            int y,
            float height
        )
        {
            _heightmap[x, y] = height;
        }

        public float GetHeight(
            int x,
            int y
        )
        {
            if (x < 0 || y < 0 || x >= _heightmap.GetLength(0) || y >= _heightmap.GetLength(1))
                return 0;

            return _heightmap[x, y];
        }
        
        public int? GetHeightLayerIndex(
            int x,
            int y,
            int step = 1)
        {
            if (step <= 0)
            {
                return null;
            }

            return Convert.ToInt32(
                Math.Round(GetHeight(x, y) / step) * step
            );
        }
        
        public void Dispose()
        {
            Array.Clear(_heightmap);
        }
    }
}
