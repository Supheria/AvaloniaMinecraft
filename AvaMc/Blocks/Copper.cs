using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Copper : BlockGen
{
    public override Block GetBlock()
    {
        return Get();
    }

    public static Block Get()
    {
        return new()
        {
            Id = BlockId.Copper,
            TextureLocation = new()
            {
                [Direction.North] = new(5, 0),
                [Direction.South] = new(5, 0),
                [Direction.East] = new(5, 0),
                [Direction.West] = new(5, 0),
                [Direction.Up] = new(5, 0),
                [Direction.Down] = new(5, 0),
            },
        };
    }
}
