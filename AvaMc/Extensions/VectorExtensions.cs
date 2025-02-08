using System.Numerics;
using Avalonia;
using AvaMc.Util;

namespace AvaMc.Extensions;

public static class VectorExtensions
{
    public static Vector2 ToVector2(this Size size)
    {
        return new((float)size.Width, (float)size.Height);
    }

    public static Vector2I Mod(this Vector2I p1, Vector2I p2)
    {
        return new(p1.X % p2.X, p1.Y % p2.Y);
    }

    public static Vector3I Mod(this Vector3I p1, Vector3I p2)
    {
        return new(p1.X % p2.X, p1.Y % p2.Y, p1.Z % p2.Z);
    }

    public static Vector2I Add(this Vector2I p1, Vector2I p2)
    {
        return Vector2I.Add(p1, p2);
    }

    public static Vector3I Add(this Vector3I p1, Vector3I p2)
    {
        return Vector3I.Add(p1, p2);
    }

    public static bool IsNaN(this Vector3 p)
    {
        return float.IsNaN(p.X) || float.IsNaN(p.Y) || float.IsNaN(p.Z);
    }

    public static Vector2I Xz(this Vector3I p)
    {
        return new(p.X, p.Z);
    }

    public static Vector3 Xyz(this Vector4 v)
    {
        return new(v.X, v.Y, v.Z);
    }
}
