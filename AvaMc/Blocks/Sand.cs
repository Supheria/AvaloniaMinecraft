using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Sand : Block
{
    // public override BlockId Id { get; } = BlockId.Sand;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(0, 1);
    }
}
