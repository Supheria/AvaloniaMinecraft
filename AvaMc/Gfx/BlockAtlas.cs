using System;
using System.IO;
using System.Numerics;
using AvaMc.Assets;
using AvaMc.Blocks;
using AvaMc.Util;
using Silk.NET.OpenGLES;
using StbImageSharp;

namespace AvaMc.Gfx;

public sealed class BlockAtlas
{
    public const int FramesPerSecond = 6;
    public const int FrameCount = 16;
    public Vector2I Size { get; }
    public Vector2I SpriteSize { get; }
    public Vector2I SizeSprites { get; }
    public Texture2D[] Frames { get; }
    public Atlas Atlas { get; set; }
    int Ticks { get; set; }

    public BlockAtlas(
        Vector2I size,
        Vector2I spriteSize,
        Vector2I sizeSprites,
        Texture2D[] frames,
        Atlas atlas
    )
    {
        Size = size;
        SpriteSize = spriteSize;
        SizeSprites = sizeSprites;
        Frames = frames;
        Atlas = atlas;
    }

    public static BlockAtlas Create(GL gl, string textureName, Vector2I spriteSize)
    {
        using var stream = AssetsRead.ReadTexture(textureName);
        return Create(gl, stream, spriteSize);
    }

    public static BlockAtlas Create(GL gl, Stream stream, Vector2I spriteSize)
    {
        var image = Texture2D.LoadImage(stream);
        var pixelsSize = image.Width * image.Height * image.ColumnNumber;
        var size = new Vector2I(image.Width, image.Height);
        var sizeSprites = Vector2I.Divide(size, spriteSize);
        
        var frames = new Texture2D[FrameCount];
        for (var i = 0; i < FrameCount; i++)
        {
            var newPixels = new byte[pixelsSize];
            Array.Copy(image.Pixels, newPixels, pixelsSize);
            foreach (var block in Block.AllBlocks())
            {
                if (!block.Animated)
                    continue;

                var offsets = block.GetAnimationFrameOffsets();
                CopyOffset(newPixels, size, spriteSize, sizeSprites, offsets[i], offsets[0], image.ColumnNumber);
            }
            frames[i] = Texture2D.Create(
                gl,
                0,
                newPixels,
                image.Width,
                image.Height,
                image.ColumnNumber
            );
        }
        var atlas = Atlas.Create(frames[0], spriteSize);
        
        return new(size, spriteSize, sizeSprites, frames, atlas);
    }

    private static void CopyOffset(
        byte[] pixels,
        Vector2I size,
        Vector2I spriteSize,
        Vector2I sizeSprites,
        Vector2I from,
        Vector2I to,
        int columnNumber
    )
    {
        CopyPixels(
            pixels,
            size,
            spriteSize,
            Vector2I.Multiply(spriteSize, new Vector2I(from.X, sizeSprites.Y - from.Y - 1)),
            Vector2I.Multiply(spriteSize, new Vector2I(to.X, sizeSprites.Y - to.Y - 1)),
            columnNumber
        );
    }

    private static void CopyPixels(
        byte[] pixels,
        Vector2I imageSize,
        Vector2I size,
        Vector2I from,
        Vector2I to,
        int columnNumber
    )
    {
        for (var j = 0; j < size.Y; j++)
        {
            for (var i = 0; i < size.X; i++)
            {
                for (var m = 0; m < columnNumber; m++)
                    pixels[((to.Y + j) * imageSize.X + (to.X + i)) * columnNumber + m] = pixels[
                        ((from.Y + j) * imageSize.X + (from.X + i)) * columnNumber + m
                    ];
            }
        }
    }

    public void Delete(GL gl)
    {
        foreach (var frame in Frames)
            frame.Delete(gl);
    }

    public void Update()
    {
        var frame = (GlobalState.Ticks / (GlobalState.TickRate / FramesPerSecond)) % FrameCount;
        Atlas.Texture = Frames[frame];
    }
}
