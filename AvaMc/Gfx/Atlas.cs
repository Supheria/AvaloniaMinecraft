using AvaMc.Extensions;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace AvaMc.Gfx;

public sealed class Atlas
{
    public Texture2D Texture { get; set; }
    Vector2D<int> Size { get; }
    Vector2D<int> SpriteSize { get; }
    public Vector2D<float> SpriteUnit { get; }

    // Vector2 PixelUnit { get; }

    private Atlas(Texture2D texture, Vector2D<int> spriteSize)
    {
        Texture = texture;
        SpriteSize = spriteSize;
        SpriteUnit = Vector2D.Divide(spriteSize.ToVector2F(), texture.Size.ToVector2F());
        // PixelUnit = Vector2.Divide(Vector2.One, texture.Size);
        Size = Vector2D.Divide(texture.Size, spriteSize);
    }

    public static Atlas Create(Texture2D texture, Vector2D<int> spriteSize)
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

    public Vector2D<float> Offset(Vector2D<int> pos)
    {
        return Vector2D.Multiply(new Vector2D<float>(pos.X, SpriteSize.Y - pos.Y - 1), SpriteUnit);
    }
}
