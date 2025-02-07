using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Gravel : Block
{
    // public override BlockId Id { get; } = BlockId.Gravel;
    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(6, 0);
    }
}