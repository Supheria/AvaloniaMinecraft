using System;
using Microsoft.Xna.Framework;

namespace AvaMc.Util;

public class Direction
{
    public const int MaxValue = 5;
    int Value
    {
        get => _value;
        set
        {
            if (value < 0 || value > MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"max value is {MaxValue}"
                );
            _value = value;
        }
    }

    int _value;

    private Direction(int value)
    {
        Value = value;
    }

    public static Direction North { get; } = new(0);
    public static Direction South { get; } = new(1);
    public static Direction East { get; } = new(2);
    public static Direction West { get; } = new(3);
    public static Direction Up { get; } = new(4);
    public static Direction Down { get; } = new(5);

    public Vector3 GetVector3()
    {
        return Value switch
        {
            0 => new(0, 0, -1),
            1 => new(0, 0, 1),
            2 => new(1, 0, 0),
            3 => new(-1, 0, 0),
            4 => new(0, 1, 0),
            5 => new(0, -1, 0),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static bool operator >=(Direction dir1, Direction dir2)
    {
        return dir1.Value > dir2.Value;
    }

    public static bool operator <=(Direction dir1, Direction dir2)
    {
        return dir1.Value <= dir2.Value;
    }

    public static Direction operator ++(Direction dir)
    {
        dir.Value++;
        return dir;
    }
}
