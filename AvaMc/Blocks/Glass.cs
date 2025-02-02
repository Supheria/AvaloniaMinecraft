using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Glass : Block
{
    public Glass()
        : base(new() { Id = BlockId.Glass, Transparent = true }) { }

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(1, 1);
    }
}
