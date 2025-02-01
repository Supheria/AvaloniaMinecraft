using System;
using Avalonia;
using Silk.NET.Maths;
using Vector3D = Silk.NET.Maths.Vector3D;

namespace AvaMc.Extensions;

public static class VectorExtensions
{
    public static Vector2D<float> ToVector2F(this Size size)
    {
        return new((float)size.Width, (float)size.Height);
    }

    public static Vector2D<T> Xz<T>(this Vector3D<T> p)
        where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new(p.X, p.Z);
    }

    public static Vector2D<int> Mod(this Vector2D<int> p1, Vector2D<int> p2)
    {
        return new(p1.X % p2.X, p1.Y % p2.Y);
    }

    public static Vector2D<T> Add<T>(this Vector2D<T> p1, Vector2D<T> p2)
        where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return Vector2D.Add(p1, p2);
    }

    public static Vector3D<int> Mod(this Vector3D<int> p1, Vector3D<int> p2)
    {
        return new(p1.X % p2.X, p1.Y % p2.Y, p1.Z % p2.Z);
    }

    public static Vector3D<T> Add<T>(this Vector3D<T> p1, Vector3D<T> p2)
        where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return Vector3D.Add(p1, p2);
    }

    public static Vector2D<float> ToVector2F(this Vector2D<int> p)
    {
        return new(p.X, p.Y);
    }

    public static Vector3D<float> ToVector3F(this Vector3D<int> p)
    {
        return new(p.X, p.Y, p.Z);
    }
}
