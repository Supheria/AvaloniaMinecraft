using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Air : Block
{
    public Air()
        : base(new() { Id = BlockId.Air, Transparent = true }) { }

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(15, 13);
    }
}
