using Avalonia;
using Microsoft.Xna.Framework;

namespace AvaMc.Extensions;

public static class VectorExtensions
{
    public static Vector2 ToVector2(this Size size)
    {
        return new((float)size.Width, (float)size.Height);
    }
}