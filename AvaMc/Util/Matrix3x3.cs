using System.Numerics;

namespace AvaMc.Util;

public struct Matrix3X3
{
    float[,] Data { get; }

    public float this[int x, int y]
    {
        get => Data[x, y];
        set => Data[x, y] = value;
    }

    public Matrix3X3()
    {
        Data = new float[3, 3]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
        };
    }

    public Matrix3X3(float[,] data)
    {
        Data = data;
    }

    public static Vector3 Multiply(Matrix3X3 m, Vector3 v)
    {
        var dest = Vector3.Zero;
        dest[0] = m[0, 0] * v[0] + m[1, 0] * v[1] + m[2, 0] * v[2];
        dest[1] = m[0, 1] * v[0] + m[1, 1] * v[1] + m[2, 1] * v[2];
        dest[2] = m[0, 2] * v[0] + m[1, 2] * v[1] + m[2, 2] * v[2];
        return dest;
    }
}
