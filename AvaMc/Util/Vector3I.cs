// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;

namespace AvaMc.Util;

/// <summary>
/// Describes a 3D-vector.
/// </summary>
// #if XNADESIGNPROVIDED
// [System.ComponentModel.TypeConverter(typeof(Microsoft.Xna.Framework.Design.Vector3TypeConverter))]
// #endif
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Vector3I : IEquatable<Vector3I>
{
    #region Private Fields

    private static readonly Vector3I zero = new Vector3I(0, 0, 0);
    private static readonly Vector3I one = new Vector3I(1, 1, 1);
    private static readonly Vector3I unitX = new Vector3I(1, 0, 0);
    private static readonly Vector3I unitY = new Vector3I(0, 1, 0);
    private static readonly Vector3I unitZ = new Vector3I(0, 0, 1);
    private static readonly Vector3I up = new Vector3I(0, 1, 0);
    private static readonly Vector3I down = new Vector3I(0, -1, 0);
    private static readonly Vector3I right = new Vector3I(1, 0, 0);
    private static readonly Vector3I left = new Vector3I(-1, 0, 0);
    private static readonly Vector3I forward = new Vector3I(0, 0, -1);
    private static readonly Vector3I backward = new Vector3I(0, 0, 1);

    #endregion

    #region Public Fields

    /// <summary>
    /// The x coordinate of this <see cref="Vector3I"/>.
    /// </summary>
    [DataMember]
    public int X;

    /// <summary>
    /// The y coordinate of this <see cref="Vector3I"/>.
    /// </summary>
    [DataMember]
    public int Y;

    /// <summary>
    /// The z coordinate of this <see cref="Vector3I"/>.
    /// </summary>
    [DataMember]
    public int Z;

    #endregion

    #region Public Properties

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components 0, 0, 0.
    /// </summary>
    public static Vector3I Zero
    {
        get { return zero; }
    }

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components 1, 1, 1.
    /// </summary>
    public static Vector3I One
    {
        get { return one; }
    }

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components 1, 0, 0.
    /// </summary>
    public static Vector3I UnitX
    {
        get { return unitX; }
    }

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components 0, 1, 0.
    /// </summary>
    public static Vector3I UnitY
    {
        get { return unitY; }
    }

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components 0, 0, 1.
    /// </summary>
    public static Vector3I UnitZ
    {
        get { return unitZ; }
    }

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components 0, 1, 0.
    /// </summary>
    public static Vector3I Up
    {
        get { return up; }
    }

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components 0, -1, 0.
    /// </summary>
    public static Vector3I Down
    {
        get { return down; }
    }

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components 1, 0, 0.
    /// </summary>
    public static Vector3I Right
    {
        get { return right; }
    }

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components -1, 0, 0.
    /// </summary>
    public static Vector3I Left
    {
        get { return left; }
    }

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components 0, 0, -1.
    /// </summary>
    public static Vector3I Forward
    {
        get { return forward; }
    }

    /// <summary>
    /// Returns a <see cref="Vector3I"/> with components 0, 0, 1.
    /// </summary>
    public static Vector3I Backward
    {
        get { return backward; }
    }

    #endregion

    #region Internal Properties

    internal string DebugDisplayString
    {
        get
        {
            return string.Concat(
                this.X.ToString(),
                "  ",
                this.Y.ToString(),
                "  ",
                this.Z.ToString()
            );
        }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Constructs a 3d vector with X, Y and Z from three values.
    /// </summary>
    /// <param name="x">The x coordinate in 3d-space.</param>
    /// <param name="y">The y coordinate in 3d-space.</param>
    /// <param name="z">The z coordinate in 3d-space.</param>
    public Vector3I(int x, int y, int z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    /// <summary>
    /// Constructs a 3d vector with X, Y and Z set to the same value.
    /// </summary>
    /// <param name="value">The x, y and z coordinates in 3d-space.</param>
    public Vector3I(int value)
    {
        this.X = value;
        this.Y = value;
        this.Z = value;
    }

    // /// <summary>
    // /// Constructs a 3d vector with X, Y from <see cref="Vector2"/> and Z from a scalar.
    // /// </summary>
    // /// <param name="value">The x and y coordinates in 3d-space.</param>
    // /// <param name="z">The z coordinate in 3d-space.</param>
    // public Vector3I(Vector2I value, int z)
    // {
    //     this.X = value.X;
    //     this.Y = value.Y;
    //     this.Z = z;
    // }

    #endregion

    #region Public Methods

    /// <summary>
    /// Performs vector addition on <paramref name="value1"/> and <paramref name="value2"/>.
    /// </summary>
    /// <param name="value1">The first vector to add.</param>
    /// <param name="value2">The second vector to add.</param>
    /// <returns>The result of the vector addition.</returns>
    public static Vector3I Add(Vector3I value1, Vector3I value2)
    {
        value1.X += value2.X;
        value1.Y += value2.Y;
        value1.Z += value2.Z;
        return value1;
    }

    /// <summary>
    /// Performs vector addition on <paramref name="value1"/> and
    /// <paramref name="value2"/>, storing the result of the
    /// addition in <paramref name="result"/>.
    /// </summary>
    /// <param name="value1">The first vector to add.</param>
    /// <param name="value2">The second vector to add.</param>
    /// <param name="result">The result of the vector addition.</param>
    public static void Add(ref Vector3I value1, ref Vector3I value2, out Vector3I result)
    {
        result.X = value1.X + value2.X;
        result.Y = value1.Y + value2.Y;
        result.Z = value1.Z + value2.Z;
    }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 3d-triangle.
    // /// </summary>
    // /// <param name="value1">The first vector of 3d-triangle.</param>
    // /// <param name="value2">The second vector of 3d-triangle.</param>
    // /// <param name="value3">The third vector of 3d-triangle.</param>
    // /// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 3d-triangle.</param>
    // /// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 3d-triangle.</param>
    // /// <returns>The cartesian translation of barycentric coordinates.</returns>
    // public static Vector3I Barycentric(Vector3I value1, Vector3I value2, Vector3I value3, float amount1, float amount2)
    // {
    //     return new Vector3I(
    //         MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2),
    //         MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2),
    //         MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2));
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 3d-triangle.
    // /// </summary>
    // /// <param name="value1">The first vector of 3d-triangle.</param>
    // /// <param name="value2">The second vector of 3d-triangle.</param>
    // /// <param name="value3">The third vector of 3d-triangle.</param>
    // /// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 3d-triangle.</param>
    // /// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 3d-triangle.</param>
    // /// <param name="result">The cartesian translation of barycentric coordinates as an output parameter.</param>
    // public static void Barycentric(ref Vector3I value1, ref Vector3I value2, ref Vector3I value3, float amount1, float amount2, out Vector3I result)
    // {
    //     result.X = MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2);
    //     result.Y = MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2);
    //     result.Z = MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2);
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains CatmullRom interpolation of the specified vectors.
    // /// </summary>
    // /// <param name="value1">The first vector in interpolation.</param>
    // /// <param name="value2">The second vector in interpolation.</param>
    // /// <param name="value3">The third vector in interpolation.</param>
    // /// <param name="value4">The fourth vector in interpolation.</param>
    // /// <param name="amount">Weighting factor.</param>
    // /// <returns>The result of CatmullRom interpolation.</returns>
    // public static Vector3I CatmullRom(Vector3I value1, Vector3I value2, Vector3I value3, Vector3I value4, float amount)
    // {
    //     return new Vector3I(
    //         MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
    //         MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount),
    //         MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount));
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains CatmullRom interpolation of the specified vectors.
    // /// </summary>
    // /// <param name="value1">The first vector in interpolation.</param>
    // /// <param name="value2">The second vector in interpolation.</param>
    // /// <param name="value3">The third vector in interpolation.</param>
    // /// <param name="value4">The fourth vector in interpolation.</param>
    // /// <param name="amount">Weighting factor.</param>
    // /// <param name="result">The result of CatmullRom interpolation as an output parameter.</param>
    // public static void CatmullRom(ref Vector3I value1, ref Vector3I value2, ref Vector3I value3, ref Vector3I value4, float amount, out Vector3I result)
    // {
    //     result.X = MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount);
    //     result.Y = MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount);
    //     result.Z = MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount);
    // }

    // /// <summary>
    // /// Round the members of this <see cref="Vector3I"/> towards positive infinity.
    // /// </summary>
    // public void Ceiling()
    // {
    //     X = MathF.Ceiling(X);
    //     Y = MathF.Ceiling(Y);
    //     Z = MathF.Ceiling(Z);
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains members from another vector rounded towards positive infinity.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/>.</param>
    // /// <returns>The rounded <see cref="Vector3I"/>.</returns>
    // public static Vector3I Ceiling(Vector3I value)
    // {
    //     value.X = MathF.Ceiling(value.X);
    //     value.Y = MathF.Ceiling(value.Y);
    //     value.Z = MathF.Ceiling(value.Z);
    //     return value;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains members from another vector rounded towards positive infinity.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/>.</param>
    // /// <param name="result">The rounded <see cref="Vector3I"/>.</param>
    // public static void Ceiling(ref Vector3I value, out Vector3I result)
    // {
    //     result.X = MathF.Ceiling(value.X);
    //     result.Y = MathF.Ceiling(value.Y);
    //     result.Z = MathF.Ceiling(value.Z);
    // }

    /// <summary>
    /// Clamps the specified value within a range.
    /// </summary>
    /// <param name="value1">The value to clamp.</param>
    /// <param name="min">The min value.</param>
    /// <param name="max">The max value.</param>
    /// <returns>The clamped value.</returns>
    public static Vector3I Clamp(Vector3I value1, Vector3I min, Vector3I max)
    {
        return new Vector3I(
            MathHelper.Clamp(value1.X, min.X, max.X),
            MathHelper.Clamp(value1.Y, min.Y, max.Y),
            MathHelper.Clamp(value1.Z, min.Z, max.Z)
        );
    }

    /// <summary>
    /// Clamps the specified value within a range.
    /// </summary>
    /// <param name="value1">The value to clamp.</param>
    /// <param name="min">The min value.</param>
    /// <param name="max">The max value.</param>
    /// <param name="result">The clamped value as an output parameter.</param>
    public static void Clamp(
        ref Vector3I value1,
        ref Vector3I min,
        ref Vector3I max,
        out Vector3I result
    )
    {
        result.X = MathHelper.Clamp(value1.X, min.X, max.X);
        result.Y = MathHelper.Clamp(value1.Y, min.Y, max.Y);
        result.Z = MathHelper.Clamp(value1.Z, min.Z, max.Z);
    }

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The cross product of two vectors.</returns>
    public static Vector3I Cross(Vector3I vector1, Vector3I vector2)
    {
        Cross(ref vector1, ref vector2, out vector1);
        return vector1;
    }

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <param name="result">The cross product of two vectors as an output parameter.</param>
    public static void Cross(ref Vector3I vector1, ref Vector3I vector2, out Vector3I result)
    {
        var x = vector1.Y * vector2.Z - vector2.Y * vector1.Z;
        var y = -(vector1.X * vector2.Z - vector2.X * vector1.Z);
        var z = vector1.X * vector2.Y - vector2.X * vector1.Y;
        result.X = x;
        result.Y = y;
        result.Z = z;
    }

    /// <summary>
    /// Returns the distance between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The distance between two vectors.</returns>
    public static float Distance(Vector3I value1, Vector3I value2)
    {
        float result;
        DistanceSquared(ref value1, ref value2, out result);
        return MathF.Sqrt(result);
    }

    // /// <summary>
    // /// Returns the distance between two vectors.
    // /// </summary>
    // /// <param name="value1">The first vector.</param>
    // /// <param name="value2">The second vector.</param>
    // /// <param name="result">The distance between two vectors as an output parameter.</param>
    // public static void Distance(ref Vector3I value1, ref Vector3I value2, out float result)
    // {
    //     DistanceSquared(ref value1, ref value2, out result);
    //     result = MathF.Sqrt(result);
    // }

    /// <summary>
    /// Returns the squared distance between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The squared distance between two vectors.</returns>
    public static int DistanceSquared(Vector3I value1, Vector3I value2)
    {
        return  (value1.X - value2.X) * (value1.X - value2.X) +
                (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                (value1.Z - value2.Z) * (value1.Z - value2.Z);
    }

    /// <summary>
    /// Returns the squared distance between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The squared distance between two vectors as an output parameter.</param>
    public static void DistanceSquared(ref Vector3I value1, ref Vector3I value2, out float result)
    {
        result = (value1.X - value2.X) * (value1.X - value2.X) +
                 (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                 (value1.Z - value2.Z) * (value1.Z - value2.Z);
    }

    /// <summary>
    /// Divides the components of a <see cref="Vector3I"/> by the components of another <see cref="Vector3I"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    /// <param name="value2">Divisor <see cref="Vector3I"/>.</param>
    /// <returns>The result of dividing the vectors.</returns>
    public static Vector3I Divide(Vector3I value1, Vector3I value2)
    {
        value1.X /= value2.X;
        value1.Y /= value2.Y;
        value1.Z /= value2.Z;
        return value1;
    }

    // /// <summary>
    // /// Divides the components of a <see cref="Vector3I"/> by a scalar.
    // /// </summary>
    // /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    // /// <param name="divider">Divisor scalar.</param>
    // /// <returns>The result of dividing a vector by a scalar.</returns>
    // public static Vector3I Divide(Vector3I value1, float divider)
    // {
    //     float factor = 1 / divider;
    //     value1.X *= factor;
    //     value1.Y *= factor;
    //     value1.Z *= factor;
    //     return value1;
    // }

    // /// <summary>
    // /// Divides the components of a <see cref="Vector3I"/> by a scalar.
    // /// </summary>
    // /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    // /// <param name="divider">Divisor scalar.</param>
    // /// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
    // public static void Divide(ref Vector3I value1, float divider, out Vector3I result)
    // {
    //     float factor = 1 / divider;
    //     result.X = value1.X * factor;
    //     result.Y = value1.Y * factor;
    //     result.Z = value1.Z * factor;
    // }

    /// <summary>
    /// Divides the components of a <see cref="Vector3I"/> by the components of another <see cref="Vector3I"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    /// <param name="value2">Divisor <see cref="Vector3I"/>.</param>
    /// <param name="result">The result of dividing the vectors as an output parameter.</param>
    public static void Divide(ref Vector3I value1, ref Vector3I value2, out Vector3I result)
    {
        result.X = value1.X / value2.X;
        result.Y = value1.Y / value2.Y;
        result.Z = value1.Z / value2.Z;
    }

    // /// <summary>
    // /// Returns a dot product of two vectors.
    // /// </summary>
    // /// <param name="value1">The first vector.</param>
    // /// <param name="value2">The second vector.</param>
    // /// <returns>The dot product of two vectors.</returns>
    // public static float Dot(Vector3I value1, Vector3I value2)
    // {
    //     return value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
    // }

    // /// <summary>
    // /// Returns a dot product of two vectors.
    // /// </summary>
    // /// <param name="value1">The first vector.</param>
    // /// <param name="value2">The second vector.</param>
    // /// <param name="result">The dot product of two vectors as an output parameter.</param>
    // public static void Dot(ref Vector3I value1, ref Vector3I value2, out float result)
    // {
    //     result = value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
    // }

    /// <summary>
    /// Compares whether current instance is equal to specified <see cref="Object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="Object"/> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public override bool Equals(object obj)
    {
        if (!(obj is Vector3I))
            return false;

        var other = (Vector3I)obj;
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    /// <summary>
    /// Compares whether current instance is equal to specified <see cref="Vector3I"/>.
    /// </summary>
    /// <param name="other">The <see cref="Vector3I"/> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public bool Equals(Vector3I other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    // /// <summary>
    // /// Round the members of this <see cref="Vector3I"/> towards negative infinity.
    // /// </summary>
    // public void Floor()
    // {
    //     X = MathF.Floor(X);
    //     Y = MathF.Floor(Y);
    //     Z = MathF.Floor(Z);
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains members from another vector rounded towards negative infinity.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/>.</param>
    // /// <returns>The rounded <see cref="Vector3I"/>.</returns>
    // public static Vector3I Floor(Vector3I value)
    // {
    //     value.X = MathF.Floor(value.X);
    //     value.Y = MathF.Floor(value.Y);
    //     value.Z = MathF.Floor(value.Z);
    //     return value;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains members from another vector rounded towards negative infinity.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/>.</param>
    // /// <param name="result">The rounded <see cref="Vector3I"/>.</param>
    // public static void Floor(ref Vector3I value, out Vector3I result)
    // {
    //     result.X = MathF.Floor(value.X);
    //     result.Y = MathF.Floor(value.Y);
    //     result.Z = MathF.Floor(value.Z);
    // }

    // /// <summary>
    // /// Gets the hash code of this <see cref="Vector3I"/>.
    // /// </summary>
    // /// <returns>Hash code of this <see cref="Vector3I"/>.</returns>
    // public override int GetHashCode() {
    //     unchecked
    //     {
    //         var hashCode = X.GetHashCode();
    //         hashCode = (hashCode * 397) ^ Y.GetHashCode();
    //         hashCode = (hashCode * 397) ^ Z.GetHashCode();
    //         return hashCode;
    //     }
    // }

    /// <summary>
    /// Gets the hash code of this <see cref="Vector3I"/>.
    /// </summary>
    /// <returns>Hash code of this <see cref="Vector3I"/>.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains hermite spline interpolation.
    // /// </summary>
    // /// <param name="value1">The first position vector.</param>
    // /// <param name="tangent1">The first tangent vector.</param>
    // /// <param name="value2">The second position vector.</param>
    // /// <param name="tangent2">The second tangent vector.</param>
    // /// <param name="amount">Weighting factor.</param>
    // /// <returns>The hermite spline interpolation vector.</returns>
    // public static Vector3I Hermite(Vector3I value1, Vector3I tangent1, Vector3I value2, Vector3I tangent2, float amount)
    // {
    //     return new Vector3I(MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount),
    //                        MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount),
    //                        MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount));
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains hermite spline interpolation.
    // /// </summary>
    // /// <param name="value1">The first position vector.</param>
    // /// <param name="tangent1">The first tangent vector.</param>
    // /// <param name="value2">The second position vector.</param>
    // /// <param name="tangent2">The second tangent vector.</param>
    // /// <param name="amount">Weighting factor.</param>
    // /// <param name="result">The hermite spline interpolation vector as an output parameter.</param>
    // public static void Hermite(ref Vector3I value1, ref Vector3I tangent1, ref Vector3I value2, ref Vector3I tangent2, float amount, out Vector3I result)
    // {
    //     result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
    //     result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
    //     result.Z = MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount);
    // }

    // /// <summary>
    // /// Returns the length of this <see cref="Vector3I"/>.
    // /// </summary>
    // /// <returns>The length of this <see cref="Vector3I"/>.</returns>
    // public float Length()
    // {
    //     return MathF.Sqrt((X * X) + (Y * Y) + (Z * Z));
    // }

    /// <summary>
    /// Returns the squared length of this <see cref="Vector3I"/>.
    /// </summary>
    /// <returns>The squared length of this <see cref="Vector3I"/>.</returns>
    public int LengthSquared()
    {
        return (X * X) + (Y * Y) + (Z * Z);
    }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains linear interpolation of the specified vectors.
    // /// </summary>
    // /// <param name="value1">The first vector.</param>
    // /// <param name="value2">The second vector.</param>
    // /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
    // /// <returns>The result of linear interpolation of the specified vectors.</returns>
    // public static Vector3I Lerp(Vector3I value1, Vector3I value2, float amount)
    // {
    //     return new Vector3I(
    //         MathHelper.Lerp(value1.X, value2.X, amount),
    //         MathHelper.Lerp(value1.Y, value2.Y, amount),
    //         MathHelper.Lerp(value1.Z, value2.Z, amount));
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains linear interpolation of the specified vectors.
    // /// </summary>
    // /// <param name="value1">The first vector.</param>
    // /// <param name="value2">The second vector.</param>
    // /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
    // /// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
    // public static void Lerp(ref Vector3I value1, ref Vector3I value2, float amount, out Vector3I result)
    // {
    //     result.X = MathHelper.Lerp(value1.X, value2.X, amount);
    //     result.Y = MathHelper.Lerp(value1.Y, value2.Y, amount);
    //     result.Z = MathHelper.Lerp(value1.Z, value2.Z, amount);
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains linear interpolation of the specified vectors.
    // /// Uses <see cref="MathHelper.LerpPrecise"/> on MathHelper for the interpolation.
    // /// Less efficient but more precise compared to <see cref="Vector3I.Lerp(VVector3I VVector3I float)"/>.
    // /// See remarks section of <see cref="MathHelper.LerpPrecise"/> on MathHelper for more info.
    // /// </summary>
    // /// <param name="value1">The first vector.</param>
    // /// <param name="value2">The second vector.</param>
    // /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
    // /// <returns>The result of linear interpolation of the specified vectors.</returns>
    // public static Vector3I LerpPrecise(Vector3I value1, Vector3I value2, float amount)
    // {
    //     return new Vector3I(
    //         MathHelper.LerpPrecise(value1.X, value2.X, amount),
    //         MathHelper.LerpPrecise(value1.Y, value2.Y, amount),
    //         MathHelper.LerpPrecise(value1.Z, value2.Z, amount));
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains linear interpolation of the specified vectors.
    // /// Uses <see cref="MathHelper.LerpPrecise"/> on MathHelper for the interpolation.
    // /// Less efficient but more precise compared to <see cref="Vector3I.Lerp(ref Vector3I, ref Vector3I, float, out Vector3I)"/>.
    // /// See remarks section of <see cref="MathHelper.LerpPrecise"/> on MathHelper for more info.
    // /// </summary>
    // /// <param name="value1">The first vector.</param>
    // /// <param name="value2">The second vector.</param>
    // /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
    // /// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
    // public static void LerpPrecise(ref Vector3I value1, ref Vector3I value2, float amount, out Vector3I result)
    // {
    //     result.X = MathHelper.LerpPrecise(value1.X, value2.X, amount);
    //     result.Y = MathHelper.LerpPrecise(value1.Y, value2.Y, amount);
    //     result.Z = MathHelper.LerpPrecise(value1.Z, value2.Z, amount);
    // }

    /// <summary>
    /// Creates a new <see cref="Vector3I"/> that contains a maximal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The <see cref="Vector3I"/> with maximal values from the two vectors.</returns>
    public static Vector3I Max(Vector3I value1, Vector3I value2)
    {
        return new Vector3I(
            MathHelper.Max(value1.X, value2.X),
            MathHelper.Max(value1.Y, value2.Y),
            MathHelper.Max(value1.Z, value2.Z)
        );
    }

    /// <summary>
    /// Creates a new <see cref="Vector3I"/> that contains a maximal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The <see cref="Vector3I"/> with maximal values from the two vectors as an output parameter.</param>
    public static void Max(ref Vector3I value1, ref Vector3I value2, out Vector3I result)
    {
        result.X = MathHelper.Max(value1.X, value2.X);
        result.Y = MathHelper.Max(value1.Y, value2.Y);
        result.Z = MathHelper.Max(value1.Z, value2.Z);
    }

    /// <summary>
    /// Creates a new <see cref="Vector3I"/> that contains a minimal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The <see cref="Vector3I"/> with minimal values from the two vectors.</returns>
    public static Vector3I Min(Vector3I value1, Vector3I value2)
    {
        return new Vector3I(
            MathHelper.Min(value1.X, value2.X),
            MathHelper.Min(value1.Y, value2.Y),
            MathHelper.Min(value1.Z, value2.Z)
        );
    }

    /// <summary>
    /// Creates a new <see cref="Vector3I"/> that contains a minimal values from the two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="result">The <see cref="Vector3I"/> with minimal values from the two vectors as an output parameter.</param>
    public static void Min(ref Vector3I value1, ref Vector3I value2, out Vector3I result)
    {
        result.X = MathHelper.Min(value1.X, value2.X);
        result.Y = MathHelper.Min(value1.Y, value2.Y);
        result.Z = MathHelper.Min(value1.Z, value2.Z);
    }

    /// <summary>
    /// Creates a new <see cref="Vector3I"/> that contains a multiplication of two vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    /// <param name="value2">Source <see cref="Vector3I"/>.</param>
    /// <returns>The result of the vector multiplication.</returns>
    public static Vector3I Multiply(Vector3I value1, Vector3I value2)
    {
        value1.X *= value2.X;
        value1.Y *= value2.Y;
        value1.Z *= value2.Z;
        return value1;
    }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains a multiplication of <see cref="Vector3I"/> and a scalar.
    // /// </summary>
    // /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    // /// <param name="scaleFactor">Scalar value.</param>
    // /// <returns>The result of the vector multiplication with a scalar.</returns>
    // public static Vector3I Multiply(Vector3I value1, float scaleFactor)
    // {
    //     value1.X *= scaleFactor;
    //     value1.Y *= scaleFactor;
    //     value1.Z *= scaleFactor;
    //     return value1;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains a multiplication of <see cref="Vector3I"/> and a scalar.
    // /// </summary>
    // /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    // /// <param name="scaleFactor">Scalar value.</param>
    // /// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
    // public static void Multiply(ref Vector3I value1, float scaleFactor, out Vector3I result)
    // {
    //     result.X = value1.X * scaleFactor;
    //     result.Y = value1.Y * scaleFactor;
    //     result.Z = value1.Z * scaleFactor;
    // }

    /// <summary>
    /// Creates a new <see cref="Vector3I"/> that contains a multiplication of two vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    /// <param name="value2">Source <see cref="Vector3I"/>.</param>
    /// <param name="result">The result of the vector multiplication as an output parameter.</param>
    public static void Multiply(ref Vector3I value1, ref Vector3I value2, out Vector3I result)
    {
        result.X = value1.X * value2.X;
        result.Y = value1.Y * value2.Y;
        result.Z = value1.Z * value2.Z;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3I"/> that contains the specified vector inversion.
    /// </summary>
    /// <param name="value">Source <see cref="Vector3I"/>.</param>
    /// <returns>The result of the vector inversion.</returns>
    public static Vector3I Negate(Vector3I value)
    {
        value = new Vector3I(-value.X, -value.Y, -value.Z);
        return value;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3I"/> that contains the specified vector inversion.
    /// </summary>
    /// <param name="value">Source <see cref="Vector3I"/>.</param>
    /// <param name="result">The result of the vector inversion as an output parameter.</param>
    public static void Negate(ref Vector3I value, out Vector3I result)
    {
        result.X = -value.X;
        result.Y = -value.Y;
        result.Z = -value.Z;
    }

    // /// <summary>
    // /// Turns this <see cref="Vector3I"/> to a unit vector with the same direction.
    // /// </summary>
    // public void Normalize()
    // {
    //     float factor = MathF.Sqrt((X * X) + (Y * Y) + (Z * Z));
    //     factor = 1f / factor;
    //     X *= factor;
    //     Y *= factor;
    //     Z *= factor;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains a normalized values from another vector.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/>.</param>
    // /// <returns>Unit vector.</returns>
    // public static Vector3I Normalize(Vector3I value)
    // {
    //     float factor = MathF.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z));
    //     factor = 1f / factor;
    //     return new Vector3I(value.X * factor, value.Y * factor, value.Z * factor);
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains a normalized values from another vector.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/>.</param>
    // /// <param name="result">Unit vector as an output parameter.</param>
    // public static void Normalize(ref Vector3I value, out Vector3I result)
    // {
    //     float factor = MathF.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z));
    //     factor = 1f / factor;
    //     result.X = value.X * factor;
    //     result.Y = value.Y * factor;
    //     result.Z = value.Z * factor;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains reflect vector of the given vector and normal.
    // /// </summary>
    // /// <param name="vector">Source <see cref="Vector3I"/>.</param>
    // /// <param name="normal">Reflection normal.</param>
    // /// <returns>Reflected vector.</returns>
    // public static Vector3I Reflect(Vector3I vector, Vector3I normal)
    // {
    //     // I is the original array
    //     // N is the normal of the incident plane
    //     // R = I - (2 * N * ( DotProduct[ I,N] ))
    //     Vector3I reflectedVector;
    //     // inline the dotProduct here instead of calling method
    //     float dotProduct = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);
    //     reflectedVector.X = vector.X - (2.0f * normal.X) * dotProduct;
    //     reflectedVector.Y = vector.Y - (2.0f * normal.Y) * dotProduct;
    //     reflectedVector.Z = vector.Z - (2.0f * normal.Z) * dotProduct;
    //
    //     return reflectedVector;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains reflect vector of the given vector and normal.
    // /// </summary>
    // /// <param name="vector">Source <see cref="Vector3I"/>.</param>
    // /// <param name="normal">Reflection normal.</param>
    // /// <param name="result">Reflected vector as an output parameter.</param>
    // public static void Reflect(ref Vector3I vector, ref Vector3I normal, out Vector3I result)
    // {
    //     // I is the original array
    //     // N is the normal of the incident plane
    //     // R = I - (2 * N * ( DotProduct[ I,N] ))
    //
    //     // inline the dotProduct here instead of calling method
    //     float dotProduct = ((vector.X * normal.X) + (vector.Y * normal.Y)) + (vector.Z * normal.Z);
    //     result.X = vector.X - (2.0f * normal.X) * dotProduct;
    //     result.Y = vector.Y - (2.0f * normal.Y) * dotProduct;
    //     result.Z = vector.Z - (2.0f * normal.Z) * dotProduct;
    // }

    // /// <summary>
    // /// Round the members of this <see cref="Vector3I"/> towards the nearest integer value.
    // /// </summary>
    // public void Round()
    // {
    //     X = MathF.Round(X);
    //     Y = MathF.Round(Y);
    //     Z = MathF.Round(Z);
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains members from another vector rounded to the nearest integer value.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/>.</param>
    // /// <returns>The rounded <see cref="Vector3I"/>.</returns>
    // public static Vector3I Round(Vector3I value)
    // {
    //     value.X = MathF.Round(value.X);
    //     value.Y = MathF.Round(value.Y);
    //     value.Z = MathF.Round(value.Z);
    //     return value;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains members from another vector rounded to the nearest integer value.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/>.</param>
    // /// <param name="result">The rounded <see cref="Vector3I"/>.</param>
    // public static void Round(ref Vector3I value, out Vector3I result)
    // {
    //     result.X = MathF.Round(value.X);
    //     result.Y = MathF.Round(value.Y);
    //     result.Z = MathF.Round(value.Z);
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains cubic interpolation of the specified vectors.
    // /// </summary>
    // /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    // /// <param name="value2">Source <see cref="Vector3I"/>.</param>
    // /// <param name="amount">Weighting value.</param>
    // /// <returns>Cubic interpolation of the specified vectors.</returns>
    // public static Vector3I SmoothStep(Vector3I value1, Vector3I value2, float amount)
    // {
    //     return new Vector3I(
    //         MathHelper.SmoothStep(value1.X, value2.X, amount),
    //         MathHelper.SmoothStep(value1.Y, value2.Y, amount),
    //         MathHelper.SmoothStep(value1.Z, value2.Z, amount));
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains cubic interpolation of the specified vectors.
    // /// </summary>
    // /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    // /// <param name="value2">Source <see cref="Vector3I"/>.</param>
    // /// <param name="amount">Weighting value.</param>
    // /// <param name="result">Cubic interpolation of the specified vectors as an output parameter.</param>
    // public static void SmoothStep(ref Vector3I value1, ref Vector3I value2, float amount, out Vector3I result)
    // {
    //     result.X = MathHelper.SmoothStep(value1.X, value2.X, amount);
    //     result.Y = MathHelper.SmoothStep(value1.Y, value2.Y, amount);
    //     result.Z = MathHelper.SmoothStep(value1.Z, value2.Z, amount);
    // }

    /// <summary>
    /// Creates a new <see cref="Vector3I"/> that contains subtraction of on <see cref="Vector3I"/> from a another.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    /// <param name="value2">Source <see cref="Vector3I"/>.</param>
    /// <returns>The result of the vector subtraction.</returns>
    public static Vector3I Subtract(Vector3I value1, Vector3I value2)
    {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
        value1.Z -= value2.Z;
        return value1;
    }

    /// <summary>
    /// Creates a new <see cref="Vector3I"/> that contains subtraction of on <see cref="Vector3I"/> from a another.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3I"/>.</param>
    /// <param name="value2">Source <see cref="Vector3I"/>.</param>
    /// <param name="result">The result of the vector subtraction as an output parameter.</param>
    public static void Subtract(ref Vector3I value1, ref Vector3I value2, out Vector3I result)
    {
        result.X = value1.X - value2.X;
        result.Y = value1.Y - value2.Y;
        result.Z = value1.Z - value2.Z;
    }

    /// <summary>
    /// Returns a <see cref="String"/> representation of this <see cref="Vector3I"/> in the format:
    /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Z:[<see cref="Z"/>]}
    /// </summary>
    /// <returns>A <see cref="String"/> representation of this <see cref="Vector3I"/>.</returns>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(32);
        sb.Append("{X:");
        sb.Append(this.X);
        sb.Append(" Y:");
        sb.Append(this.Y);
        sb.Append(" Z:");
        sb.Append(this.Z);
        sb.Append("}");
        return sb.ToString();
    }

    #region Transform

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains a transformation of 3d-vector by the specified <see cref="Matrix4"/>.
    // /// </summary>
    // /// <param name="position">Source <see cref="Vector3I"/>.</param>
    // /// <param name="matrix">The transformation <see cref="Matrix4"/>.</param>
    // /// <returns>Transformed <see cref="Vector3I"/>.</returns>
    // public static Vector3I Transform(Vector3I position, Matrix4 matrix)
    // {
    //     Transform(ref position, ref matrix, out position);
    //     return position;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains a transformation of 3d-vector by the specified <see cref="Matrix4"/>.
    // /// </summary>
    // /// <param name="position">Source <see cref="Vector3I"/>.</param>
    // /// <param name="matrix">The transformation <see cref="Matrix4"/>.</param>
    // /// <param name="result">Transformed <see cref="Vector3I"/> as an output parameter.</param>
    // public static void Transform(ref Vector3I position, ref Matrix4 matrix, out Vector3I result)
    // {
    //     var x = (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41;
    //     var y = (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42;
    //     var z = (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43;
    //     result.X = x;
    //     result.Y = y;
    //     result.Z = z;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains a transformation of 3d-vector by the specified <see cref="Quaternion"/>, representing the rotation.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/>.</param>
    // /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
    // /// <returns>Transformed <see cref="Vector3I"/>.</returns>
    // public static Vector3I Transform(Vector3I value, Quaternion rotation)
    // {
    //     Vector3I result;
    //     Transform(ref value, ref rotation, out result);
    //     return result;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains a transformation of 3d-vector by the specified <see cref="Quaternion"/>, representing the rotation.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/>.</param>
    // /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
    // /// <param name="result">Transformed <see cref="Vector3I"/> as an output parameter.</param>
    // public static void Transform(ref Vector3I value, ref Quaternion rotation, out Vector3I result)
    // {
    //     float x = 2 * (rotation.Y * value.Z - rotation.Z * value.Y);
    //     float y = 2 * (rotation.Z * value.X - rotation.X * value.Z);
    //     float z = 2 * (rotation.X * value.Y - rotation.Y * value.X);
    //
    //     result.X = value.X + x * rotation.W + (rotation.Y * z - rotation.Z * y);
    //     result.Y = value.Y + y * rotation.W + (rotation.Z * x - rotation.X * z);
    //     result.Z = value.Z + z * rotation.W + (rotation.X * y - rotation.Y * x);
    // }

    // /// <summary>
    // /// Apply transformation on vectors within array of <see cref="Vector3I"/> by the specified <see cref="Matrix4"/> and places the results in an another array.
    // /// </summary>
    // /// <param name="sourceArray">Source array.</param>
    // /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
    // /// <param name="matrix">The transformation <see cref="Matrix4"/>.</param>
    // /// <param name="destinationArray">Destination array.</param>
    // /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector3I"/> should be written.</param>
    // /// <param name="length">The number of vectors to be transformed.</param>
    // public static void Transform(Vector3I[] sourceArray, int sourceIndex, ref Matrix4 matrix, Vector3I[] destinationArray, int destinationIndex, int length)
    // {
    //     if (sourceArray == null)
    //         throw new ArgumentNullException("sourceArray");
    //     if (destinationArray == null)
    //         throw new ArgumentNullException("destinationArray");
    //     if (sourceArray.Length < sourceIndex + length)
    //         throw new ArgumentException("Source array length is lesser than sourceIndex + length");
    //     if (destinationArray.Length < destinationIndex + length)
    //         throw new ArgumentException("Destination array length is lesser than destinationIndex + length");
    //
    //     // TODO: Are there options on some platforms to implement a vectorized version of this?
    //
    //     for (var i = 0; i < length; i++)
    //     {
    //         var position = sourceArray[sourceIndex + i];
    //         destinationArray[destinationIndex + i] =
    //             new Vector3I(
    //                 (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41,
    //                 (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42,
    //                 (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43);
    //     }
    // }

    // /// <summary>
    // /// Apply transformation on vectors within array of <see cref="Vector3I"/> by the specified <see cref="Quaternion"/> and places the results in an another array.
    // /// </summary>
    // /// <param name="sourceArray">Source array.</param>
    // /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
    // /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
    // /// <param name="destinationArray">Destination array.</param>
    // /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector3I"/> should be written.</param>
    // /// <param name="length">The number of vectors to be transformed.</param>
    // public static void Transform(Vector3I[] sourceArray, int sourceIndex, ref Quaternion rotation, Vector3I[] destinationArray, int destinationIndex, int length)
    // {
    //     if (sourceArray == null)
    //         throw new ArgumentNullException("sourceArray");
    //     if (destinationArray == null)
    //         throw new ArgumentNullException("destinationArray");
    //     if (sourceArray.Length < sourceIndex + length)
    //         throw new ArgumentException("Source array length is lesser than sourceIndex + length");
    //     if (destinationArray.Length < destinationIndex + length)
    //         throw new ArgumentException("Destination array length is lesser than destinationIndex + length");
    //
    //     // TODO: Are there options on some platforms to implement a vectorized version of this?
    //
    //     for (var i = 0; i < length; i++)
    //     {
    //         var position = sourceArray[sourceIndex + i];
    //
    //         float x = 2 * (rotation.Y * position.Z - rotation.Z * position.Y);
    //         float y = 2 * (rotation.Z * position.X - rotation.X * position.Z);
    //         float z = 2 * (rotation.X * position.Y - rotation.Y * position.X);
    //
    //         destinationArray[destinationIndex + i] =
    //             new Vector3I(
    //                 position.X + x * rotation.W + (rotation.Y * z - rotation.Z * y),
    //                 position.Y + y * rotation.W + (rotation.Z * x - rotation.X * z),
    //                 position.Z + z * rotation.W + (rotation.X * y - rotation.Y * x));
    //     }
    // }

    // /// <summary>
    // /// Apply transformation on all vectors within array of <see cref="Vector3I"/> by the specified <see cref="Matrix4"/> and places the results in an another array.
    // /// </summary>
    // /// <param name="sourceArray">Source array.</param>
    // /// <param name="matrix">The transformation <see cref="Matrix4"/>.</param>
    // /// <param name="destinationArray">Destination array.</param>
    // public static void Transform(Vector3I[] sourceArray, ref Matrix4 matrix, Vector3I[] destinationArray)
    // {
    //     if (sourceArray == null)
    //         throw new ArgumentNullException("sourceArray");
    //     if (destinationArray == null)
    //         throw new ArgumentNullException("destinationArray");
    //     if (destinationArray.Length < sourceArray.Length)
    //         throw new ArgumentException("Destination array length is lesser than source array length");
    //
    //     // TODO: Are there options on some platforms to implement a vectorized version of this?
    //
    //     for (var i = 0; i < sourceArray.Length; i++)
    //     {
    //         var position = sourceArray[i];
    //         destinationArray[i] =
    //             new Vector3I(
    //                 (position.X*matrix.M11) + (position.Y*matrix.M21) + (position.Z*matrix.M31) + matrix.M41,
    //                 (position.X*matrix.M12) + (position.Y*matrix.M22) + (position.Z*matrix.M32) + matrix.M42,
    //                 (position.X*matrix.M13) + (position.Y*matrix.M23) + (position.Z*matrix.M33) + matrix.M43);
    //     }
    // }

    // /// <summary>
    // /// Apply transformation on all vectors within array of <see cref="Vector3I"/> by the specified <see cref="Quaternion"/> and places the results in an another array.
    // /// </summary>
    // /// <param name="sourceArray">Source array.</param>
    // /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
    // /// <param name="destinationArray">Destination array.</param>
    // public static void Transform(Vector3I[] sourceArray, ref Quaternion rotation, Vector3I[] destinationArray)
    // {
    //     if (sourceArray == null)
    //         throw new ArgumentNullException("sourceArray");
    //     if (destinationArray == null)
    //         throw new ArgumentNullException("destinationArray");
    //     if (destinationArray.Length < sourceArray.Length)
    //         throw new ArgumentException("Destination array length is lesser than source array length");
    //
    //     // TODO: Are there options on some platforms to implement a vectorized version of this?
    //
    //     for (var i = 0; i < sourceArray.Length; i++)
    //     {
    //         var position = sourceArray[i];
    //
    //         float x = 2 * (rotation.Y * position.Z - rotation.Z * position.Y);
    //         float y = 2 * (rotation.Z * position.X - rotation.X * position.Z);
    //         float z = 2 * (rotation.X * position.Y - rotation.Y * position.X);
    //
    //         destinationArray[i] =
    //             new Vector3I(
    //                 position.X + x * rotation.W + (rotation.Y * z - rotation.Z * y),
    //                 position.Y + y * rotation.W + (rotation.Z * x - rotation.X * z),
    //                 position.Z + z * rotation.W + (rotation.X * y - rotation.Y * x));
    //     }
    // }

    #endregion

    #region TransformNormal

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains a transformation of the specified normal by the specified <see cref="Matrix4"/>.
    // /// </summary>
    // /// <param name="normal">Source <see cref="Vector3I"/> which represents a normal vector.</param>
    // /// <param name="matrix">The transformation <see cref="Matrix4"/>.</param>
    // /// <returns>Transformed normal.</returns>
    // public static Vector3I TransformNormal(Vector3I normal, Matrix4 matrix)
    // {
    //     TransformNormal(ref normal, ref matrix, out normal);
    //     return normal;
    // }

    // /// <summary>
    // /// Creates a new <see cref="Vector3I"/> that contains a transformation of the specified normal by the specified <see cref="Matrix4"/>.
    // /// </summary>
    // /// <param name="normal">Source <see cref="Vector3I"/> which represents a normal vector.</param>
    // /// <param name="matrix">The transformation <see cref="Matrix4"/>.</param>
    // /// <param name="result">Transformed normal as an output parameter.</param>
    // public static void TransformNormal(ref Vector3I normal, ref Matrix4 matrix, out Vector3I result)
    // {
    //     var x = (normal.X * matrix.M11) + (normal.Y * matrix.M21) + (normal.Z * matrix.M31);
    //     var y = (normal.X * matrix.M12) + (normal.Y * matrix.M22) + (normal.Z * matrix.M32);
    //     var z = (normal.X * matrix.M13) + (normal.Y * matrix.M23) + (normal.Z * matrix.M33);
    //     result.X = x;
    //     result.Y = y;
    //     result.Z = z;
    // }

    // /// <summary>
    // /// Apply transformation on normals within array of <see cref="Vector3I"/> by the specified <see cref="Matrix4"/> and places the results in an another array.
    // /// </summary>
    // /// <param name="sourceArray">Source array.</param>
    // /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
    // /// <param name="matrix">The transformation <see cref="Matrix4"/>.</param>
    // /// <param name="destinationArray">Destination array.</param>
    // /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector3I"/> should be written.</param>
    // /// <param name="length">The number of normals to be transformed.</param>
    // public static void TransformNormal(Vector3I[] sourceArray,
    //  int sourceIndex,
    //  ref Matrix4 matrix,
    //  Vector3I[] destinationArray,
    //  int destinationIndex,
    //  int length)
    // {
    //     if (sourceArray == null)
    //         throw new ArgumentNullException("sourceArray");
    //     if (destinationArray == null)
    //         throw new ArgumentNullException("destinationArray");
    //     if(sourceArray.Length < sourceIndex + length)
    //         throw new ArgumentException("Source array length is lesser than sourceIndex + length");
    //     if (destinationArray.Length < destinationIndex + length)
    //         throw new ArgumentException("Destination array length is lesser than destinationIndex + length");
    //
    //     for (int x = 0; x < length; x++)
    //     {
    //         var normal = sourceArray[sourceIndex + x];
    //
    //         destinationArray[destinationIndex + x] =
    //              new Vector3I(
    //                 (normal.X * matrix.M11) + (normal.Y * matrix.M21) + (normal.Z * matrix.M31),
    //                 (normal.X * matrix.M12) + (normal.Y * matrix.M22) + (normal.Z * matrix.M32),
    //                 (normal.X * matrix.M13) + (normal.Y * matrix.M23) + (normal.Z * matrix.M33));
    //     }
    // }

    // /// <summary>
    // /// Apply transformation on all normals within array of <see cref="Vector3I"/> by the specified <see cref="Matrix4"/> and places the results in an another array.
    // /// </summary>
    // /// <param name="sourceArray">Source array.</param>
    // /// <param name="matrix">The transformation <see cref="Matrix4"/>.</param>
    // /// <param name="destinationArray">Destination array.</param>
    // public static void TransformNormal(Vector3I[] sourceArray, ref Matrix4 matrix, Vector3I[] destinationArray)
    // {
    //     if(sourceArray == null)
    //         throw new ArgumentNullException("sourceArray");
    //     if (destinationArray == null)
    //         throw new ArgumentNullException("destinationArray");
    //     if (destinationArray.Length < sourceArray.Length)
    //         throw new ArgumentException("Destination array length is lesser than source array length");
    //
    //     for (var i = 0; i < sourceArray.Length; i++)
    //     {
    //         var normal = sourceArray[i];
    //
    //         destinationArray[i] =
    //             new Vector3I(
    //                 (normal.X*matrix.M11) + (normal.Y*matrix.M21) + (normal.Z*matrix.M31),
    //                 (normal.X*matrix.M12) + (normal.Y*matrix.M22) + (normal.Z*matrix.M32),
    //                 (normal.X*matrix.M13) + (normal.Y*matrix.M23) + (normal.Z*matrix.M33));
    //     }
    // }

    #endregion

    /// <summary>
    /// Deconstruction method for <see cref="Vector3I"/>.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void Deconstruct(out float x, out float y, out float z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    /// <summary>
    /// Returns a <see cref="System.Numerics.Vector3"/>.
    /// </summary>
    public Vector3 ToNumerics()
    {
        return new Vector3(this.X, this.Y, this.Z);
    }

    #endregion

    #region Operators

    // /// <summary>
    // /// Converts a <see cref="System.Numerics.Vector3"/> to a <see cref="Vector3I"/>.
    // /// </summary>
    // /// <param name="value">The converted value.</param>
    // public static implicit operator Vector3I(System.Numerics.Vector3 value)
    // {
    //     return new Vector3I(value.X, value.Y, value.Z);
    // }

    /// <summary>
    /// Compares whether two <see cref="Vector3I"/> instances are equal.
    /// </summary>
    /// <param name="value1"><see cref="Vector3I"/> instance on the left of the equal sign.</param>
    /// <param name="value2"><see cref="Vector3I"/> instance on the right of the equal sign.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public static bool operator ==(Vector3I value1, Vector3I value2)
    {
        return value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z;
    }

    /// <summary>
    /// Compares whether two <see cref="Vector3I"/> instances are not equal.
    /// </summary>
    /// <param name="value1"><see cref="Vector3I"/> instance on the left of the not equal sign.</param>
    /// <param name="value2"><see cref="Vector3I"/> instance on the right of the not equal sign.</param>
    /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
    public static bool operator !=(Vector3I value1, Vector3I value2)
    {
        return !(value1 == value2);
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3I"/> on the left of the add sign.</param>
    /// <param name="value2">Source <see cref="Vector3I"/> on the right of the add sign.</param>
    /// <returns>Sum of the vectors.</returns>
    public static Vector3I operator +(Vector3I value1, Vector3I value2)
    {
        value1.X += value2.X;
        value1.Y += value2.Y;
        value1.Z += value2.Z;
        return value1;
    }

    /// <summary>
    /// Inverts values in the specified <see cref="Vector3I"/>.
    /// </summary>
    /// <param name="value">Source <see cref="Vector3I"/> on the right of the sub sign.</param>
    /// <returns>Result of the inversion.</returns>
    public static Vector3I operator -(Vector3I value)
    {
        value = new Vector3I(-value.X, -value.Y, -value.Z);
        return value;
    }

    /// <summary>
    /// Subtracts a <see cref="Vector3I"/> from a <see cref="Vector3I"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3I"/> on the left of the sub sign.</param>
    /// <param name="value2">Source <see cref="Vector3I"/> on the right of the sub sign.</param>
    /// <returns>Result of the vector subtraction.</returns>
    public static Vector3I operator -(Vector3I value1, Vector3I value2)
    {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
        value1.Z -= value2.Z;
        return value1;
    }

    /// <summary>
    /// Multiplies the components of two vectors by each other.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3I"/> on the left of the mul sign.</param>
    /// <param name="value2">Source <see cref="Vector3I"/> on the right of the mul sign.</param>
    /// <returns>Result of the vector multiplication.</returns>
    public static Vector3I operator *(Vector3I value1, Vector3I value2)
    {
        value1.X *= value2.X;
        value1.Y *= value2.Y;
        value1.Z *= value2.Z;
        return value1;
    }

    // /// <summary>
    // /// Multiplies the components of vector by a scalar.
    // /// </summary>
    // /// <param name="value">Source <see cref="Vector3I"/> on the left of the mul sign.</param>
    // /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
    // /// <returns>Result of the vector multiplication with a scalar.</returns>
    // public static Vector3I operator *(Vector3I value, float scaleFactor)
    // {
    //     value.X *= scaleFactor;
    //     value.Y *= scaleFactor;
    //     value.Z *= scaleFactor;
    //     return value;
    // }

    // /// <summary>
    // /// Multiplies the components of vector by a scalar.
    // /// </summary>
    // /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
    // /// <param name="value">Source <see cref="Vector3I"/> on the right of the mul sign.</param>
    // /// <returns>Result of the vector multiplication with a scalar.</returns>
    // public static Vector3I operator *(float scaleFactor, Vector3I value)
    // {
    //     value.X *= scaleFactor;
    //     value.Y *= scaleFactor;
    //     value.Z *= scaleFactor;
    //     return value;
    // }

    /// <summary>
    /// Divides the components of a <see cref="Vector3I"/> by the components of another <see cref="Vector3I"/>.
    /// </summary>
    /// <param name="value1">Source <see cref="Vector3I"/> on the left of the div sign.</param>
    /// <param name="value2">Divisor <see cref="Vector3I"/> on the right of the div sign.</param>
    /// <returns>The result of dividing the vectors.</returns>
    public static Vector3I operator /(Vector3I value1, Vector3I value2)
    {
        value1.X /= value2.X;
        value1.Y /= value2.Y;
        value1.Z /= value2.Z;
        return value1;
    }

    // /// <summary>
    // /// Divides the components of a <see cref="Vector3I"/> by a scalar.
    // /// </summary>
    // /// <param name="value1">Source <see cref="Vector3I"/> on the left of the div sign.</param>
    // /// <param name="divider">Divisor scalar on the right of the div sign.</param>
    // /// <returns>The result of dividing a vector by a scalar.</returns>
    // public static Vector3I operator /(Vector3I value1, float divider)
    // {
    //     float factor = 1 / divider;
    //     value1.X *= factor;
    //     value1.Y *= factor;
    //     value1.Z *= factor;
    //     return value1;
    // }

    #endregion
}
