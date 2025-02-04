using System.Numerics;
using AvaMc.Gfx;
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
        var offsets = new Vector2I[BlockAtlas.FrameCount];
        for (var i = 0; i < offsets.Length; i ++)
        {
            offsets[i] = new(i, 15);
        }
        return offsets;
    }
}
