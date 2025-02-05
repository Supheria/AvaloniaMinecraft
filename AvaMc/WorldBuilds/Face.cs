using System.Numerics;

namespace AvaMc.WorldBuilds;

public sealed class Face
{
    public uint[] Indices { get; }
    public Vector3 Position { get; }
    public float DistanceSquared { get; private set; }

    public Face(uint[] indices, Vector3 position)
    {
        Indices = indices;
        Position = position;
    }
    public void SetDistance(Vector3 center)
    {
        DistanceSquared = Vector3.DistanceSquared(center, Position);
    }
}
