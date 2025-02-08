using AvaMc.Util;

namespace AvaMc.Coordinates;

public readonly struct BlockHeightmapPosition
{
    Vector2I Value { get; }
    
    public int X => Value.X;
    public int Z => Value.Y;

    public BlockHeightmapPosition(Vector2I value)
    {
        Value = value;
    }
}
