// using System;
// using System.IO;
// using Avalonia.Platform;
// using AvaMc.Assets;
// using AvaMc.Blocks;
// using Microsoft.Xna.Framework;
// using Silk.NET.OpenGLES;
//
// namespace AvaMc.Gfx;
//
// public sealed class BlockAtlas
// {
//     public const int FramesPerSecond = 6;
//     public const int FrameCounts = 16;
//     public Vector2 Size { get; }
//
//     // TODO: ?
//     public Vector2 SizeSprites { get; }
//     public Vector2 SpriteSize { get; }
//     public Texture2D[] Frames { get; }
//     public Atlas Atlas { get; set; }
//
//     public BlockAtlas(
//         Vector2 size,
//         Vector2 sizeSprites,
//         Vector2 spriteSize,
//         Texture2D[] frames,
//         Atlas atlas
//     )
//     {
//         Size = size;
//         SizeSprites = sizeSprites;
//         SpriteSize = spriteSize;
//         Frames = frames;
//         Atlas = atlas;
//     }
//     
//     public static BlockAtlas Create(GL gl, string textureName, Vector2 spriteSize)
//     {
//         using var stream = AssetsRead.ReadTexture(textureName);
//         return Create(gl, stream, spriteSize);
//     }
//
//     public static BlockAtlas Create(GL gl, Stream stream, Vector2 spriteSize)
//     {
//         var image = Texture2D.LoadImage(stream);
//         var pixelsSize = image.Width * image.Height * image.ColumnNumber;
//         var size = new Vector2(image.Width, image.Height);
//         var sizeSprites = Vector2.Divide(size, spriteSize);
//         var frames = new Texture2D[FrameCounts];
//         for (var i = 0; i < FrameCounts; i++)
//         {
//             var newPixels = new byte[pixelsSize];
//             Array.Copy(image.Pixels, newPixels, pixelsSize);
//             foreach (var block in Block.Blocks.Values)
//             {
//                 if (block.Id is BlockId.Air || !block.Animated)
//                     continue;
//                 var offsets = block.GetAnimationFrameOffsets();
//                 // TODO: misunderstand here
//                 CopyOffset(newPixels, size, spriteSize, sizeSprites, offsets[i], offsets[0]);
//             }
//             frames[i] = Texture2D.Create(
//                 gl,
//                 0,
//                 newPixels,
//                 image.Width,
//                 image.Height,
//                 image.ColumnNumber
//             );
//         }
//         var atlas = Atlas.Create(frames[0], spriteSize);
//         return new(size, sizeSprites, spriteSize, frames, atlas);
//     }
//
//     private static void CopyOffset(
//         byte[] pixels,
//         Vector2 size,
//         Vector2 spriteSize,
//         Vector2 sizeSprites,
//         Vector2 from,
//         Vector2 to
//     )
//     {
//         CopyPixels(
//             pixels,
//             size,
//             spriteSize,
//             Vector2.Multiply(spriteSize, new Vector2(from.X, sizeSprites.Y - from.Y - 1)),
//             Vector2.Multiply(spriteSize, new Vector2(to.X, sizeSprites.Y - to.Y - 1))
//         );
//     }
//
//     private static void CopyPixels(
//         byte[] pixels,
//         Vector2 imageSize,
//         Vector2 size,
//         Vector2 from,
//         Vector2 to
//     )
//     {
//         for (var j = 0; j < size.Y; j++)
//         {
//             for (var i = 0; i < size.X; i++)
//             {
//                 pixels[(((int)to.Y + j) * (int)imageSize.X + ((int)to.X + i)) * 4] = pixels[
//                     (((int)from.X + j) * (int)imageSize.X + ((int)from.X + i)) * 4
//                 ];
//             }
//         }
//     }
//
//     public void Delete(GL gl)
//     {
//         foreach (var frame in Frames)
//             frame.Delete(gl);
//     }
//
//     public void Update()
//     {
//         var frame = (State.Ticks / (State.TickRate / FramesPerSecond)) % FrameCounts;
//         Atlas.Texture = Frames[frame];
//     }
// }
