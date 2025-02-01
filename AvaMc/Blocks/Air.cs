using AvaMc.Util;
using Silk.NET.Maths;

namespace AvaMc.Blocks;

public sealed class Air : Block
{
    public Air()
        : base(new() { Id = BlockId.Air, Transparent = true }) { }

    public override Vector2D<int> GetTextureLocation(Direction direction)
    {
        return Vector2D<int>.Zero;
    }
}
