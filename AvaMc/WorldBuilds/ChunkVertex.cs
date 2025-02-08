using System.Numerics;
using AvaMc.Gfx;

namespace AvaMc.WorldBuilds;

public readonly struct ChunkVertex
{
    public Vector3 Position { get; }
    public Vector2 Uv { get; }
    public uint Light {get;}

    public ChunkVertex(Vector3 position, Vector2 uv, uint light)
    {
        Position = position;
        Uv = uv;
        Light = light;
    }

    // public ChunkVertex(Vector3 position, Vector2 uv)
    // {
    //     Position = position;
    //     Uv = uv;
    // }
}
