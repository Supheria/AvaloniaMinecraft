using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Air : BlockGen
{
    public override Block GetBlock()
    {
        return Get();
    }
    
    public static Block Get()
    {
        return new()
        {
            Id = BlockId.Air,
            Transparent = true,
            TextureLocation = new()
            {
                [Direction.North] = new(15, 13),
                [Direction.South] = new(15, 13),
                [Direction.East] = new(15, 13),
                [Direction.West] = new(15, 13),
                [Direction.Up] = new(15, 13),
                [Direction.Down] = new(15, 13),
            },
        };
    }
}
