using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Grass : BlockGen
{
    public override Block GetBlock()
    {
        return Get();
    }

    public static Block Get()
    {
        return new()
        {
            Id = BlockId.Grass,
            TextureLocation = new()
            {
                [Direction.North] = new(1, 0),
                [Direction.South] = new(1, 0),
                [Direction.East] = new(1, 0),
                [Direction.West] = new(1, 0),
                [Direction.Up] = new(0, 0),
                [Direction.Down] = new(2, 0),
            },
        };
    }
}
