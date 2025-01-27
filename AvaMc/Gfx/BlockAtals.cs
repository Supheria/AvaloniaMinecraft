using System.IO;
using Microsoft.Xna.Framework;

namespace AvaMc.Gfx;

public sealed class BlockAtals
{
    public const int FrameCounts = 16;
    public Vector2 Size { get; }
    // TODO: ?
    public Vector2 SizeSprites { get; }
    public Vector2 SpriteSize { get; }
    public Texture2D[] Frames { get; } = new Texture2D[FrameCounts];
    public Atlas Atlas { get; set; }
    public int Ticks { get; set; }

    public BlockAtals(Stream stream, Vector2 spriteSize)
    {
        SpriteSize = spriteSize;
        var image = Texture2D.LoadImage(stream);
        Size = new(image.Width, image.Height);
        SizeSprites = Vector2.Divide(Size, spriteSize);
        for (var i = 0; i < FrameCounts; i++)
        {
            for (var n = 0; n < )
        }
    }
}
