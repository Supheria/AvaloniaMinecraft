using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Coal : Block
{
    public override BlockId Id { get; } = BlockId.Coal;
    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(4, 0);
    }
}