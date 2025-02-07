using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Dirt : Block
{
    // public override BlockId Id { get; } = BlockId.Dirt;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(2, 0);
    }
}
