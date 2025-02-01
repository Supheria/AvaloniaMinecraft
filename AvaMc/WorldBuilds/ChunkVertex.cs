using Silk.NET.Maths;

namespace AvaMc.WorldBuilds;

public readonly struct ChunkVertex
{
    public Vector3D<float> Position { get; }
    public Vector2D<float> Uv { get; }

    // public Vector3 Color {get;}

    // public ChunkVertex(Vector3 position, Vector2 uv, Vector3 color)
    // {
    //     Position = position;
    //     Uv = uv;
    //     Color = color;
    // }

    public ChunkVertex(Vector3D<float> position, Vector2D<float> uv)
    {
        Position = position;
        Uv = uv;
    }
}
