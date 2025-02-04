using System.Numerics;

namespace AvaMc.WorldBuilds;

public struct Face
{
    public int IndicesBase { get; set; }
    public Vector3 Position { get; set; }
    public float DistanceSquared { get; set; }

    public void SetDistance(Vector3 center)
    {
        DistanceSquared = Vector3.DistanceSquared(center, Position);
    }
}
