using System;
using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Util;

public readonly struct Direction
{
    public enum Type : byte
    {
        North = 0,
        South = 1,
        East = 2,
        West = 3,
        Up = 4,
        Down = 5,
    }

    public Type Value { get; }
    public int X { get; }
    public int Y { get; }
    public int Z { get; }

    private Direction(Type value)
    {
        Value = value;
        switch (value)
        {
            case Type.North:
                // Vector3F = new(0, 0, -1);
                // Value3 = new(0, 0, -1);
                Z = -1;
                break;
            case Type.South:
                // Vector3F = new(0, 0, 1);
                // Value3 = new(0, 0, 1);
                Z = 1;
                break;
            case Type.East:
                // Vector3F = new(1, 0, 0);
                // Value3 = new(1, 0, 0);
                X = 1;
                break;
            case Type.West:
                // Vector3F = new(-1, 0, 0);
                // Value3 = new(-1, 0, 0);
                X = -1;
                break;
            case Type.Up:
                // Vector3F = new(0, 1, 0);
                // Value3 = new(0, 1, 0);
                Y = 1;
                break;
            case Type.Down:
                // Vector3F = new(0, -1, 0);
                // Value3 = new(0, -1, 0);
                Y = -1;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    public static Direction Default => North;

    /// <summary>
    /// 0, 0, -1
    /// </summary>
    public static Direction North { get; } = new(Type.North);

    /// <summary>
    /// 0, 0, 1
    /// </summary>
    public static Direction South { get; } = new(Type.South);

    /// <summary>
    /// 1, 0, 0
    /// </summary>
    public static Direction East { get; } = new(Type.East);

    /// <summary>
    /// -1, 0, 0
    /// </summary>
    public static Direction West { get; } = new(Type.West);

    /// <summary>
    /// 0, 1, 0
    /// </summary>
    public static Direction Up { get; } = new(Type.Up);

    /// <summary>
    /// 0, -1, 0
    /// </summary>
    public static Direction Down { get; } = new(Type.Down);
    public static Direction[] AllDirections { get; } = [North, South, East, West, Up, Down];
    public static Direction[] AllDirectionsRevers { get; } = [Down, Up, West, East, South, North];
    public static Direction[] DirectionsXz { get; } = [North, South, East, West];

    public static Direction ToDirection(Vector3I v)
    {
        foreach (var direction in AllDirections)
        {
            if (direction.X == v.X && direction.Y == v.Y && direction.Z == v.Z)
                return direction;
        }
        throw new ArgumentException();
    }

    public static int operator *(Direction dir, int value)
    {
        return (int)dir.Value * value;
    }

    public override string ToString()
    {
        return $"{Value}({X},{Y},{Z})";
    }

    public static implicit operator int(Direction d)
    {
        return (int)d.Value;
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
