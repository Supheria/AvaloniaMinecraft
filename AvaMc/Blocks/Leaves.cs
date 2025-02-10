using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Leaves : Block
{
    public override BlockId Id { get; } = BlockId.Leaves;
    public override bool Transparent { get; } = true;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(4, 1);
    }
}
