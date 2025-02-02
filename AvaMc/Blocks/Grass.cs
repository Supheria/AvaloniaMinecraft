using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Grass : Block
{
    public Grass()
        : base(new() { Id = BlockId.Dirt, Transparent = false }) { }

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return direction.Value switch
        {
            Direction.Type.Up => new(0, 0),
            Direction.Type.Down => new(2, 0),
            _ => new(1, 0),
        };
    }
}
