using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Buttercup : Block
{
    public override BlockId Id { get; } = BlockId.Buttercup;
    public override bool Transparent { get; } = true;
    public override bool Sprite { get; } = true;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(1, 3);
    }
}
