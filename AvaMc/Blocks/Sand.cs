using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Sand : BlockGen
{
    public override Block GetBlock()
    {
        return Get();
    }
    public static Block Get()
    {
        return new()
        {
            Id = BlockId.Sand,
            TextureLocation = new()
            {
                [Direction.North] = new(0, 1),
                [Direction.South] = new(0, 1),
                [Direction.East] = new(0, 1),
                [Direction.West] = new(0, 1),
                [Direction.Up] = new(0, 1),
                [Direction.Down] = new(0, 1),
            },
        };
    }
}
