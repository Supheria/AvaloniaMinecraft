using System.IO;
using Avalonia;
using Microsoft.Xna.Framework;
using Silk.NET.OpenGLES;

namespace AvaMc.Gfx;

public sealed class Atlas
{
    Texture2D Texture { get; }
    Vector2 Size { get; }
    Vector2 SpriteSize { get; }
    Vector2 SpriteUnit { get; }
    Vector2 PixelUnit { get; }

    public Atlas(Texture2D texture, Vector2 spriteSize)
    {
        Texture = texture;
        SpriteSize = spriteSize;
        SpriteUnit = Vector2.Divide(spriteSize, texture.Size);
        PixelUnit = Vector2.Divide(Vector2.One, texture.Size);
        Size = Vector2.Divide(texture.Size, spriteSize);
    }

    public void GetUvRange(Vector2 position, ref Vector2 uvMin, ref Vector2 uvMax)
    {
        var x = position.X * SpriteSize.X;
        var y = (Size.Y - position.Y - 1) * SpriteSize.Y;
        var pMin = new Vector2(x, y);

        uvMin = Vector2.Divide(pMin, Texture.Size);
        uvMax = Vector2.Divide(Vector2.Add(pMin, SpriteSize), Texture.Size);
    }
    
    public void Delete(GL gl)
    {
        Texture.Delete(gl);
    }
}
