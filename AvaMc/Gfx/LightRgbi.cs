using System.Numerics;

namespace AvaMc.Gfx;

public struct LightRgbi
{
    public const int ChannelCount = 4;
    public int Red
    {
        get => this[0];
        set => this[0] = value;
    }
    public int Green
    {
        get => this[1];
        set => this[1] = value;
    }
    public int Blue
    {
        get => this[2];
        set => this[2] = value;
    }
    public int Intensity
    {
        get => this[3];
        set => this[3] = value;
    }
    int Channels { get; set; } = 0;

    public static LightRgbi Zero { get; } = new();

    public LightRgbi()
        : this(0, 0, 0, 0) { }

    public LightRgbi(int red, int green, int blue, int intensity)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Intensity = intensity;
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

    public Vector4 GetChannels()
    {
        return new(Red, Green, Blue, Intensity);
    }
}
