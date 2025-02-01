using System.Numerics;
using AvaMc.Extensions;
using Microsoft.Xna.Framework;
using Silk.NET.OpenGLES;

namespace AvaMc.Gfx;

public sealed class Atlas
{
    public Texture2D Texture { get; set; }
    Vector2I Size { get; }
    Vector2I SpriteSize { get; }
    public Vector2 SpriteUnit { get; }

    // Vector2 PixelUnit { get; }

    private Atlas(Texture2D texture, Vector2I spriteSize)
    {
        Texture = texture;
        SpriteSize = spriteSize;
        SpriteUnit = Vector2.Divide(spriteSize.ToNumerics(), texture.Size.ToNumerics());
        // PixelUnit = Vector2.Divide(Vector2.One, texture.Size);
        Size = Vector2I.Divide(texture.Size, spriteSize);
    }

    public static Atlas Create(Texture2D texture, Vector2I spriteSize)
    {
        return new(texture, spriteSize);
    }

    // public void GetUvRange(Vector2 position, ref Vector2 uvMin, ref Vector2 uvMax)
    // {
    //     var x = position.X * SpriteSize.X;
    //     var y = (Size.Y - position.Y - 1) * SpriteSize.Y;
    //     var pMin = new Vector2(x, y);
    //
    //     uvMin = Vector2.Divide(pMin, Texture.Size);
    //     uvMax = Vector2.Divide(Vector2.Add(pMin, SpriteSize), Texture.Size);
    // }

    public void Delete(GL gl)
    {
        Texture.Delete(gl);
    }

    public Vector2 Offset(Vector2I pos)
    {
        return Vector2.Multiply(pos.ToNumerics(), SpriteUnit);
    }
}
