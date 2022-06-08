using System.Numerics;

namespace SquadTopography.SquadMcMapDataExtractor;

internal sealed class JsonMapInfoExtra
{
    public float GetFinalScale()
    {
        var range = (Scale.Y - Scale.X) / 10000;
        return (512 * range * Scale.Z) / 512;
    }

    public Vector3 Scale { get; set; }
    public Vector2 Levels { get; set; }
    public List<Vector2> MiniMap { get; set; } = new();
    public Vector2 HDim { get; set; }
    public Vector2 LOrigin { get; set; }
}