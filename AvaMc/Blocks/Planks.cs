using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Planks : BlockGen
{
    public override Block Get()
    {
        return new()
        {
            Id = BlockId.Planks,
            TextureLocation = new()
            {
                [Direction.North] = new(6, 1),
                [Direction.South] = new(6, 1),
                [Direction.East] = new(6, 1),
                [Direction.West] = new(6, 1),
                [Direction.Up] = new(6, 1),
                [Direction.Down] = new(6, 1),
            },
        };
    }
}
