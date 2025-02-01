using AvaMc.Util;
using Silk.NET.Maths;

namespace AvaMc.Blocks;

public sealed class Dirt : Block
{
    public Dirt()
        : base(new() { Id = BlockId.Dirt, Transparent = false }) { }

    public override Vector2D<int> GetTextureLocation(Direction direction)
    {
        return new(2, 0);
    }
}
