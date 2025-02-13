using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Clay : BlockGen
{
    public override Block Get()
    {
        return new()
        {
            Id = BlockId.Clay,
            TextureLocation = new()
            {
                [Direction.North] = new(5, 1),
                [Direction.South] = new(5, 1),
                [Direction.East] = new(5, 1),
                [Direction.West] = new(5, 1),
                [Direction.Up] = new(5, 1),
                [Direction.Down] = new(5, 1),
            },
        };
    }
}
