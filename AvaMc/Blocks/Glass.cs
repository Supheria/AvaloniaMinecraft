using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Glass : Block
{
    // public override BlockId Id { get; } = BlockId.Glass;
    public override bool Transparent { get; } = true;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(1, 1);
    }
}
