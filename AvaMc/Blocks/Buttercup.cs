using System.Numerics;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Blocks;

public sealed class Buttercup : BlockGen
{
    public override Block GetBlock()
    {
        return Get();
    }
    
    public static Block Get()
    {
        return new()
        {
            Id = BlockId.Buttercup,
            Transparent = true,
            MeshType = BlockMeshType.Sprite,
            TextureLocation = new()
            {
                [Direction.North] = new(1, 3),
                [Direction.South] = new(1, 3),
                [Direction.East] = new(1, 3),
                [Direction.West] = new(1, 3),
                [Direction.Up] = new(1, 3),
                [Direction.Down] = new(1, 3),
            },
        };
    }
}
