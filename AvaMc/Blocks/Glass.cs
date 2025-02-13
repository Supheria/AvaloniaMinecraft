using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Glass : BlockGen
{
    public override Block Get()
    {
        return new()
        {
            Id = BlockId.Glass,
            Transparent = true,
            TextureLocation = new()
            {
                [Direction.North] = new(1, 1),
                [Direction.South] = new(1, 1),
                [Direction.East] = new(1, 1),
                [Direction.West] = new(1, 1),
                [Direction.Up] = new(1, 1),
                [Direction.Down] = new(1, 1),
            },
        };
    }
}
