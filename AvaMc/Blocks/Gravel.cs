using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Gravel : BlockGen
{
    public override Block Get()
    {
        return new()
        {
            Id = BlockId.Gravel,
            TextureLocation = new()
            {
                [Direction.North] = new(6, 0),
                [Direction.South] = new(6, 0),
                [Direction.East] = new(6, 0),
                [Direction.West] = new(6, 0),
                [Direction.Up] = new(6, 0),
                [Direction.Down] = new(6, 0),
            },
        };
    }
}
