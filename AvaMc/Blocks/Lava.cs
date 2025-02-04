using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Blocks;

public sealed class Lava : Block
{
    public override BlockId Id { get; } = BlockId.Lava;
    public override bool Transparent { get; } = true;
    public override bool Animated { get; } = true;
    public override bool Liquid { get; } = true;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(0, 14);
    }

    public override Vector2I[] GetAnimationFrameOffsets()
    {
        return [new(0, 14), new(1, 14), new(2, 14), new(3, 14), new(4, 14)];
    }
}
