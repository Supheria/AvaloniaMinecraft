using AvaMc.Util;
using Microsoft.Xna.Framework;

namespace AvaMc.Blocks;

public sealed class Air : Block
{
    public Air()
        : base(new() { Id = BlockId.Air, Transparent = true }) { }

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return Vector2I.Zero;
    }
}
