using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Rose : Block
{
    // public override BlockId Id { get; } = BlockId.Rose;
    public override bool Transparent { get; } = true;
    public override bool Sprite { get; } = true;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(0, 3);
    }
}
