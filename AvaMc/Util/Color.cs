using System;
using System.Numerics;
using AvaMc.Extensions;
using Silk.NET.Maths;

namespace AvaMc.Util;

public sealed class Color
{
    // Conversion code adapted from https://gist.github.com/mattatz/44f081cac87e2f7c8980#file-labcolorspace-cginc-L29

    private static Vector3 RgbToXyz(Vector3 c)
    {
        var tmp = Vector3.Zero;
        tmp.X = c.X > 0.04045f ? MathF.Pow((c.X + 0.055f) / 1.055f, 2.4f) : c.X / 12.92f;
        tmp.Y = c.Y > 0.04045f ? MathF.Pow((c.Y + 0.055f) / 1.055f, 2.4f) : c.Y / 12.92f;
        tmp.Z = c.Z > 0.04045f ? MathF.Pow((c.Z + 0.055f) / 1.055f, 2.4f) : c.Z / 12.92f;
        // var mat = new Matrix3X3(
        //     new[,]
        //     {
        //         { 0.4124f, 0.3576f, 0.1805f },
        //         { 0.2126f, 0.7152f, 0.0722f },
        //         { 0.0193f, 0.1192f, 0.9505f },
        //     }
        // );       
        var mat = new Matrix3X3(
            new[,]
            {
                { 0.4124f, 0.2126f, 0.0193f },
                { 0.3576f, 0.7152f, 0.1192f },
                { 0.1805f, 0.0722f, 0.9505f },
            }
        );
        return Vector3.Multiply(Matrix3X3.Multiply(mat, tmp), 100.0f);
    }

    private static Vector3 XyzToLab(Vector3 c)
    {
        var n = Vector3.Divide(c, new Vector3(95.047f, 100f, 108.883f));
        var v = Vector3.Zero;
        v.X = n.X > 0.008856f ? MathF.Pow(n.X, 1.0f / 3.0f) : 7.787f * n.X + 16.0f / 116.0f;
        v.Y = n.Y > 0.008856f ? MathF.Pow(n.Y, 1.0f / 3.0f) : 7.787f * n.Y + 16.0f / 116.0f;
        v.Z = n.Z > 0.008856f ? MathF.Pow(n.Z, 1.0f / 3.0f) : 7.787f * n.Z + 16.0f / 116.0f;
        return new(116.0f * v.Y - 16.0f, 500.0f * (v.X - v.Y), 200.0f * (v.Y - v.Z));
    }

    private static Vector3 RgbToLab(Vector3 c)
    {
        var lab = XyzToLab(RgbToXyz(c));
        return new(lab.X / 100.0f, 0.5f + 0.5f * (lab.Y / 127.0f), 0.5f + 0.5f * (lab.Z / 127.0f));
    }

    private static Vector3 LabToXyz(Vector3 c)
    {
        var fy = (c.X + 16.0f) / 116.0f;
        var fx = c.Y / 500.0f + fy;
        var fz = fy - c.Z / 200.0f;
        return new(
            95.047f * (fx > 0.206897f ? fx * fx * fx : (fx - 16.0f / 116.0f) / 7.787f),
            100.000f * (fy > 0.206897f ? fy * fy * fy : (fy - 16.0f / 116.0f) / 7.787f),
            108.883f * (fz > 0.206897f ? fz * fz * fz : (fz - 16.0f / 116.0f) / 7.787f)
        );
    }

    private static Vector3 XyzToRgb(Vector3 c)
    {
        // var mat = new Matrix3X3(
        //     new[,]
        //     {
        //         { 3.2406f, -1.5372f, -0.4986f },
        //         { -0.9689f, 1.8758f, 0.0415f },
        //         { 0.0557f, -0.2040f, 1.0570f },
        //     }
        // );
        var mat = new Matrix3X3(
            new[,]
            {
                { 3.2406f, -0.9689f, 0.0557f },
                { -1.5372f, 1.8758f, -0.2040f },
                { -0.4986f, 0.0415f, 1.0570f },
            }
        );
        var v = Matrix3X3.Multiply(mat, Vector3.Multiply(c, 1.0f / 100.0f));
        var r = Vector3.Zero;
        r.X = v.X > 0.0031308f ? 1.055f * MathF.Pow(v.X, 1.0f / 2.4f) - 0.055f : 12.92f * v.X;
        r.Y = v.Y > 0.0031308f ? 1.055f * MathF.Pow(v.Y, 1.0f / 2.4f) - 0.055f : 12.92f * v.Y;
        r.Z = v.Z > 0.0031308f ? 1.055f * MathF.Pow(v.Z, 1.0f / 2.4f) - 0.055f : 12.92f * v.Z;
        return r;
    }

    private static Vector3 LabToRgb(Vector3 c)
    {
        return XyzToRgb(
            LabToXyz(new(100.0f * c.X, 2.0f * 127.0f * (c.Y - 0.5f), 2.0f * 127.0f * (c.Z - 0.5f)))
        );
    }

    public static Vector3 RgbBrighten(Vector3 rgb, float d)
    {
        var lab = RgbToLab(rgb);
        return LabToRgb(new(lab.X + d, lab.Y, lab.Z));
    }

    public static Vector4 RgbaBrighten(Vector4 rgba, float d)
    {
        return new(RgbBrighten(new(rgba.X, rgba.Y, rgba.Z), d), rgba.W);
    }

    public static Vector4 RgbaLerp(Vector4 rgbaA, Vector4 rgbaB, float t)
    {
        var labA = RgbToLab(rgbaA.Xyz());
        var labB = RgbToLab(rgbaB.Xyz());

        return new(
            LabToRgb(
                new(
                    MathHelper.LerpPrecise(labA.X, labB.X, t),
                    MathHelper.LerpPrecise(labA.Y, labB.Y, t),
                    MathHelper.LerpPrecise(labA.Z, labB.Z, t)
                )
            ),
            MathHelper.LerpPrecise(rgbaA.W, rgbaB.W, t)
        );
    }

    public static Vector4 RgbaLerp3(Vector4 rgbaA, Vector4 rgbaB, Vector4 rgbaC, float t)
    {
        if (t <= 0.5f)
        {
            return RgbaLerp(rgbaA, rgbaB, t * 2.0f);
        }

        return RgbaLerp(rgbaB, rgbaC, (t - 0.5f) * 2.0f);
    }
}
