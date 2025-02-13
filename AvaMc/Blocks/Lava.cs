using System.Numerics;
using AvaMc.Gfx;
using AvaMc.Util;
using AvaMc.WorldBuilds;
using Hexa.NET.Utilities;

namespace AvaMc.Blocks;

public sealed unsafe class Lava : BlockGen
{
    public override Block Get()
    {
        return new()
        {
            Id = BlockId.Lava,
            Transparent = true,
            Animated = true,
            Liquid = true,
            MeshType = BlockMeshType.Liquid,
            TextureLocation = new()
            {
                [Direction.North] = new(0, 14),
                [Direction.South] = new(0, 14),
                [Direction.East] = new(0, 14),
                [Direction.West] = new(0, 14),
                [Direction.Up] = new(0, 14),
                [Direction.Down] = new(0, 14),
            },
            FrameOffsets = GetFrameOffsets(),
        };
    }

    private static Vector2I* GetFrameOffsets()
    {
        var offsets = Utils.AllocT<Vector2I>(BlockAtlas.FrameCount);
        for (var i = 0; i < BlockAtlas.FrameCount; i++)
        {
            offsets[i] = new(i, 14);
        }
        return offsets;
    }
}
