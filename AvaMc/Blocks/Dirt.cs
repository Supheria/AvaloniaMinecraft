using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Dirt : BlockGen
{
    public override Block GetBlock()
    {
        return Get();
    }

    public static Block Get()
    {
        return new()
        {
            Id = BlockId.Dirt,
            TextureLocation = new()
            {
                [Direction.North] = new(2, 0),
                [Direction.South] = new(2, 0),
                [Direction.East] = new(2, 0),
                [Direction.West] = new(2, 0),
                [Direction.Up] = new(2, 0),
                [Direction.Down] = new(2, 0),
            },
        };
    }
}
