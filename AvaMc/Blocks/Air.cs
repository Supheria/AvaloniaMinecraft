using AvaMc.Util;
using Microsoft.Xna.Framework;

namespace AvaMc.Blocks;

public sealed class Air : Block
{
    public Air()
        : base(new() { Id = BlockId.Air, Transparent = true }) { }

    public override Vector2 GetTextureLocation(Direction direction)
    {
        return Vector2.Zero;
    }
}
