using System;
using System.Numerics;
using AvaMc.Util;

namespace AvaMc.Gfx;

public struct LightIbgrs : IEquatable<LightIbgrs>
{
    public const int ChannelCount = 5;
    public const int SunlightChannel = 4;

    public int Intensity
    {
        get => this[0];
        set => this[0] = value;
    }
    public int Blue
    {
        get => this[1];
        set => this[1] = value;
    }
    public int Green
    {
        get => this[2];
        set => this[2] = value;
    }
    public int Red
    {
        get => this[3];
        set => this[3] = value;
    }
    public int Sunlight
    {
        get => this[SunlightChannel];
        set => this[SunlightChannel] = value;
    }
    int Channels { get; set; }

    public static LightIbgrs Zero { get; } = new();

    public LightIbgrs()
    {
        Channels = 0;
    }

    public LightIbgrs(int intensity, int blue, int green, int red, int sunlight)
    {
        Intensity = intensity;
        Green = green;
        Blue = blue;
        Red = red;
        Sunlight = sunlight;
    }

    public int this[int channel]
    {
        get => (Channels & Mask(channel)) >> Offset(channel);
        set => Channels = (Channels & ~Mask(channel)) | (value << Offset(channel));
    }

    private static int Mask(int channel)
    {
        return 0xF << channel * 4;
    }

    private static int Offset(int channel)
    {
        return channel * 4;
    }

    public uint GetChannels(Direction direction)
    {
        return (uint)(Channels | (int)direction.Value << 20);
    }

    public bool Equals(LightIbgrs other)
    {
        return Channels == other.Channels;
    }

    public override bool Equals(object? obj)
    {
        return obj is LightIbgrs other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Channels;
    }

    public static bool operator ==(LightIbgrs left, LightIbgrs right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LightIbgrs left, LightIbgrs right)
    {
        return !(left == right);
    }
}
