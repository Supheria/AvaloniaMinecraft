using System.Numerics;
using AvaMc.Gfx;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Blocks;

public sealed class Torch : BlockGen
{
    public override Block GetBlock()
    {
        return Get();
    }
    public static Block Get()
    {
        return new()
        {
            Id = BlockId.Torch,
            Transparent = true,
            CanEmitLight = true,
            MeshType = BlockMeshType.Torch,
            TorchLight = new(15, 15, 15, 15),
            TextureLocation = new()
            {
                [Direction.North] = new(0, 2),
                [Direction.South] = new(0, 2),
                [Direction.East] = new(0, 2),
                [Direction.West] = new(0, 2),
                [Direction.Up] = new(0, 2),
                [Direction.Down] = new(0, 2),
            },
        };
    }
}
