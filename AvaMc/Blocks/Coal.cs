using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Coal : BlockGen
{
    public override Block Get()
    {
        return new()
        {
            Id = BlockId.Coal,
            TextureLocation = new()
            {
                [Direction.North] = new(4, 0),
                [Direction.South] = new(4, 0),
                [Direction.East] = new(4, 0),
                [Direction.West] = new(4, 0),
                [Direction.Up] = new(4, 0),
                [Direction.Down] = new(4, 0),
            },
        };
    }
}
