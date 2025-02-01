using System.Numerics;

namespace AvaMc.WorldBuilds;

public readonly struct ChunkVertex
{
    public Vector3 Position { get; }
    public Vector2 Uv { get; }

    // public Vector3 Color {get;}

    // public ChunkVertex(Vector3 position, Vector2 uv, Vector3 color)
    // {
    //     Position = position;
    //     Uv = uv;
    //     Color = color;
    // }

    public ChunkVertex(Vector3 position, Vector2 uv)
    {
        Position = position;
        Uv = uv;
    }
}
