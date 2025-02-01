using AvaMc.Util;
using Microsoft.Xna.Framework;

namespace AvaMc.Blocks;

public sealed class Dirt : Block
{
    public Dirt()
        : base(new() { Id = BlockId.Dirt, Transparent = false }) { }

    public override Vector2 GetTextureLocation(Direction direction)
    {
        return new(2, 0);
    }
}
