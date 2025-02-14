using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Log : BlockGen
{
    public override Block GetBlock()
    {
        return Get();
    }
    public static Block Get()
    {
        return new()
        {
            Id = BlockId.Log,
            TextureLocation = new()
            {
                [Direction.North] = new(2, 1),
                [Direction.South] = new(2, 1),
                [Direction.East] = new(2, 1),
                [Direction.West] = new(2, 1),
                [Direction.Up] = new(3, 1),
                [Direction.Down] = new(3, 1),
            },
        };
    }
}
