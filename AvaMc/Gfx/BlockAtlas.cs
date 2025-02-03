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
    public const int FramesPerSecond = 5;
    public const int FrameCount = 5;
    static Vector2I AtlasSize { get; } = new(256, 256);
    static Vector2I AtlasSizeSprites { get; } = new(16, 16);
    static Vector2I AtlasSpriteSize { get; } = new(16, 16);

    // public Vector2 Size { get; }
    // public Vector2 SizeSprites { get; }
    // public Vector2 SpriteSize { get; }
    public Texture2D[] Frames { get; }
    public Atlas Atlas { get; set; }
    int Ticks { get; set; }

    public BlockAtlas(
        // Vector2 size,
        // Vector2 sizeSprites,
        // Vector2 spriteSize,
        Texture2D[] frames,
        Atlas atlas
    )
    {
        // Size = size;
        // SizeSprites = sizeSprites;
        // SpriteSize = spriteSize;
        Frames = frames;
        Atlas = atlas;
    }

    public static BlockAtlas Create(GL gl, string textureName)
    {
        using var stream = AssetsRead.ReadTexture(textureName);
        return Create(gl, stream);
    }

    public static BlockAtlas Create(GL gl, Stream stream)
    {
        var image = Texture2D.LoadImage(stream);
        var pixelsSize = image.Width * image.Height * image.ColumnNumber;
        // var size = new Vector2(image.Width, image.Height);
        // var sizeSprites = Vector2.Divide(size, spriteSize);
        var frames = new Texture2D[FrameCount];
        for (var i = 0; i < FrameCount; i++)
        {
            var newPixels = new byte[pixelsSize];
            Array.Copy(image.Pixels, newPixels, pixelsSize);
            foreach (var block in Block.Blocks.Values)
            {
                if (!block.Animated)
                    continue;

                var offsets = block.GetAnimationFrameOffsets();
                CopyOffset(newPixels, offsets[i], offsets[0], image.ColumnNumber);
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
        var atlas = Atlas.Create(frames[0], AtlasSpriteSize);
        return new(frames, atlas);
    }

    private static void CopyOffset(
        byte[] pixels,
        // Vector2 size,
        // Vector2 spriteSize,
        // Vector2 sizeSprites,
        Vector2I from,
        Vector2I to,
        int columnNumber
    )
    {
        // CopyPixels(
        //     pixels,
        //     size,
        //     spriteSize,
        //     Vector2.Multiply(spriteSize, new Vector2(from.X, sizeSprites.Y - from.Y - 1)),
        //     Vector2.Multiply(spriteSize, new Vector2(to.X, sizeSprites.Y - to.Y - 1))
        // );
        CopyPixels(
            pixels,
            AtlasSize,
            AtlasSpriteSize,
            Vector2I.Multiply(
                AtlasSpriteSize,
                new Vector2I(from.X, AtlasSizeSprites.Y - from.Y - 1)
            ),
            Vector2I.Multiply(AtlasSpriteSize, new Vector2I(to.X, AtlasSizeSprites.Y - to.Y - 1)),
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

    public void Tick()
    {
        var frame = (Ticks % 60) / (60 / FrameCount);
        Atlas.Texture = Frames[frame];
        Ticks++;
    }
}
