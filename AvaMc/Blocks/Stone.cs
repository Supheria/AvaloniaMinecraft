using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Stone : BlockGen
{
    public override Block Get()
    {
        return new()
        {
            Id = BlockId.Stone,
            TextureLocation = new()
            {
                [Direction.North] = new(3, 0),
                [Direction.South] = new(3, 0),
                [Direction.East] = new(3, 0),
                [Direction.West] = new(3, 0),
                [Direction.Up] = new(3, 0),
                [Direction.Down] = new(3, 0),
            },
        };
    }
}
