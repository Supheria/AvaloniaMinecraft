using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Water : Block
{
    public override BlockId Id { get; } = BlockId.Water;
    public override bool Transparent { get; } = true;
    public override bool Animated { get; } = true;
    public override bool Liquid { get; } = true;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(0, 15);
    }

    public override Vector2I[] GetAnimationFrameOffsets()
    {
        return [new(0, 15), new(1, 15), new(2, 15), new(3, 15), new(4, 15)];
    }
}
