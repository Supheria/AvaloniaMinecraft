using System;
using System.Numerics;
using Microsoft.Xna.Framework;

namespace AvaMc.Util;

public sealed class Direction
{
    public enum Type
    {
        North = 0,
        South = 1,
        East = 2,
        West = 3,
        Up = 4,
        Down = 5,
    }

    public Type Value { get; }
    public Vector3 Vector3F { get; }
    public Vector3I Vector3I { get; }

    private Direction(Type value)
    {
        Value = value;
        switch (value)
        {
            case Type.North:
                Vector3F = new(0, 0, -1);
                Vector3I = new(0, 0, -1);
                break;
            case Type.South:
                Vector3F = new(0, 0, 1);
                Vector3I = new(0, 0, 1);
                break;
            case Type.East:
                Vector3F = new(1, 0, 0);
                Vector3I = new(1, 0, 0);
                break;
            case Type.West:
                Vector3F = new(-1, 0, 0);
                Vector3I = new(-1, 0, 0);
                break;
            case Type.Up:
                Vector3F = new(0, 1, 0);
                Vector3I = new(0, 1, 0);
                break;
            case Type.Down:
                Vector3F = new(0, -1, 0);
                Vector3I = new(0, -1, 0);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    public static Direction North { get; } = new(Type.North);
    public static Direction South { get; } = new(Type.South);
    public static Direction East { get; } = new(Type.East);
    public static Direction West { get; } = new(Type.West);
    public static Direction Up { get; } = new(Type.Up);
    public static Direction Down { get; } = new(Type.Down);
    public static Direction[] AllDirections { get; } = [North, South, East, West, Up, Down];

    public static int operator *(Direction dir, int value)
    {
        return (int)dir.Value * value;
    }

    // public static bool operator >=(Direction dir1, Direction dir2)
    // {
    //     return dir1.Value > dir2.Value;
    // }
    //
    // public static bool operator <=(Direction dir1, Direction dir2)
    // {
    //     return dir1.Value <= dir2.Value;
    // }

    // public static Direction operator ++(Direction dir)
    // {
    //     dir.Value++;
    //     return dir;
    // }
}
