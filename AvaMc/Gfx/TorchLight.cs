namespace AvaMc.Gfx;

public struct TorchLight
{
    public const int ChannelCount = 4;

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
    int Channels { get; set; }

    public static TorchLight Zero { get; } = new();

    public TorchLight()
    {
        Channels = 0;
    }
    
    public TorchLight(int channels)
    {
        Channels = channels;
    }

    public TorchLight(int intensity, int blue, int green, int red)
    {
        Intensity = intensity;
        Green = green;
        Blue = blue;
        Red = red;
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
    
    public static implicit operator int(TorchLight light)
    {
        return light.Channels;
    }
    
    public static explicit operator TorchLight(int channes)
    {
        return new(channes);
    }
}