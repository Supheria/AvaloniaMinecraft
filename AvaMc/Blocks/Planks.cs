using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Planks : Block
{
    // public override BlockId Id { get; } = BlockId.Planks;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(6, 1);
    }
}
