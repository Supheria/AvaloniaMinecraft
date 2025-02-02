using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Dirt : Block
{
    public Dirt()
        : base(new() { Id = BlockId.Dirt, Transparent = false }) { }

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(2, 0);
    }
}
