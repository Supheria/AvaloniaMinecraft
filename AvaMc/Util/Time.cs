using System;

namespace AvaMc.Util;

public sealed class Time
{
    public const long NanosecondsPerSecond = 1000000000;
    public static long Now()
    {
        var now = DateTime.UtcNow;
        var ticks = now.Ticks;
        return ticks * TimeSpan.NanosecondsPerTick;
    }
}