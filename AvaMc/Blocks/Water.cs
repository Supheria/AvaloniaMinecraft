using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Water : Block
{
    public Water()
        : base(new() { Id = BlockId.Water, Transparent = true }) { }

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(0, 15);
    }
}
