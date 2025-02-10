using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Log : Block
{
    public override BlockId Id { get; } = BlockId.Log;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return direction.Value switch
        {
            Direction.Type.Up or Direction.Type.Down => new(3, 1),
            _ => new(2, 1),
        };
    }
}
