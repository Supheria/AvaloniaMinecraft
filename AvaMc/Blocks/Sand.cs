using AvaMc.Util;
using Silk.NET.Maths;

namespace AvaMc.Blocks;

public sealed class Sand : Block
{
    public Sand()
        : base(new() { Id = BlockId.Sand, Transparent = false }) { }

    public override Vector2D<int> GetTextureLocation(Direction direction)
    {
        return new(0, 1);
    }
}
