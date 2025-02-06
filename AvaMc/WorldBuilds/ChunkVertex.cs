using System.Numerics;
using AvaMc.Gfx;

namespace AvaMc.WorldBuilds;

public readonly struct ChunkVertex
{
    public Vector3 Position { get; }
    public Vector2 Uv { get; }
    public Vector4 LightRgbi {get;}

    public ChunkVertex(Vector3 position, Vector2 uv, LightRgbi lightRgbi)
    {
        Position = position;
        Uv = uv;
        LightRgbi = lightRgbi.GetChannels();
    }

    // public ChunkVertex(Vector3 position, Vector2 uv)
    // {
    //     Position = position;
    //     Uv = uv;
    // }
}
