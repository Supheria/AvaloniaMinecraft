using AvaMc.Util;
using Microsoft.Xna.Framework;

namespace AvaMc.Blocks;

public sealed class Sand : Block
{
    public Sand()
        : base(new() { Id = BlockId.Sand, Transparent = false }) { }

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(0, 1);
    }
}
