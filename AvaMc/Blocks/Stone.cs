using AvaMc.Util;
using Microsoft.Xna.Framework;

namespace AvaMc.Blocks;

public sealed class Stone : Block
{
    public Stone()
        : base(new() { Id = BlockId.Stone, Transparent = false }) { }

    public override Vector2 GetTextureLocation(Direction direction)
    {
        return new(3, 0);
    }
}
