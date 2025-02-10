using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Copper : Block
{
    public override BlockId Id { get; } = BlockId.Copper;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(5, 0);
    }
}
