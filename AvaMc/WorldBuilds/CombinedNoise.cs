namespace AvaMc.WorldBuilds;

// Combined noise where compute(x, z) = n.compute(x + m.compute(x, z), z)
public sealed class CombinedNoise
{
    Noise N1 { get; }
    Noise N2 { get; }

    public CombinedNoise(Noise n1, Noise n2)
    {
        N1 = n1;
        N2 = n2;
    }

    public float Compute(float seed, float x, float z)
    {
        var n2 = N2.Compute(seed, x, z);
        return N1.Compute(seed, x + n2, z);
    }
}
