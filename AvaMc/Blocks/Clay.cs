using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Clay : Block
{
    // public override BlockId Id { get; } = BlockId.Clay;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(5, 1);
    }
}
