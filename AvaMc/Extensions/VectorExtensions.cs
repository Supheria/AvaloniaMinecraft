using System;
using Avalonia;
using Microsoft.Xna.Framework;

namespace AvaMc.Extensions;

public static class VectorExtensions
{
    public static Vector2 ToVector2(this Size size)
    {
        return new((float)size.Width, (float)size.Height);
    }

    public static Vector2 Xz(this Vector3 p)
    {
        return new(p.X, p.Z);
    }

    public static Vector2 Mod(this Vector2 p1, Vector2 p2)
    {
        return new((int)p1.X % (int)p2.X, (int)p1.Y % (int)p2.Y);
    }
    
    public static Vector2 Add(this Vector2 p1, Vector2 p2)
    {
        return Vector2.Add(p1, p2);
    }
}
