using AvaMc.Util;
using Silk.NET.Maths;

namespace AvaMc.Blocks;

public sealed class Stone : Block
{
    public Stone()
        : base(new() { Id = BlockId.Stone, Transparent = false }) { }

    public override Vector2D<int> GetTextureLocation(Direction direction)
    {
        return new(3, 0);
    }
}
