using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Air : Block
{
    public override BlockId Id { get; } = BlockId.Air;
    public override bool Transparent { get; } = true;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(15, 13);
    }
}
