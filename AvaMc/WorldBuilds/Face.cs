using System.Numerics;

namespace AvaMc.WorldBuilds;

public sealed class Face
{
    public int IndicesBase { get; set; }
    public Vector3 Position { get; set; }
    public float DistanceSquared { get; set; }
}
