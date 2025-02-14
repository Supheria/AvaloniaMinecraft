using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Leaves : BlockGen
{
    public override Block GetBlock()
    {
        return Get();
    }

    public static Block Get()
    {
        return new()
        {
            Id = BlockId.Leaves,
            Transparent = true,
            TextureLocation = new()
            {
                [Direction.North] = new(4, 1),
                [Direction.South] = new(4, 1),
                [Direction.East] = new(4, 1),
                [Direction.West] = new(4, 1),
                [Direction.Up] = new(4, 1),
                [Direction.Down] = new(4, 1),
            },
        };
    }
}
