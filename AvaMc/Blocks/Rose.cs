using System.Numerics;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Blocks;

public sealed class Rose : BlockGen
{
    public override Block Get()
    {
        return new()
        {
            Id = BlockId.Rose,
            Transparent = true,
            MeshType = BlockMeshType.Sprite,
            TextureLocation = new()
            {
                [Direction.North] = new(0, 3),
                [Direction.South] = new(0, 3),
                [Direction.East] = new(0, 3),
                [Direction.West] = new(0, 3),
                [Direction.Up] = new(0, 3),
                [Direction.Down] = new(0, 3),
            },
        };
    }
}
