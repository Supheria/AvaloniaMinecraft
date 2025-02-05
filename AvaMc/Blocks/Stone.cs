using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Stone : Block
{
    // public override BlockId Id { get; } = BlockId.Stone;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(3, 0);
    }
}
