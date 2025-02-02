namespace AvaMc.WorldBuilds;

public sealed class Noise
{
    int OctaveCount { get; }
    int SeedOffset { get; }

    public Noise(int octaveCount, int seedOffset)
    {
        OctaveCount = octaveCount;
        SeedOffset = seedOffset;
    }

    public float Compute(float seed, float x, float z)
    {
        var u = 1f;
        var v = 0f;
        for (var i = 0; i < OctaveCount; i++)
        {
            v += NoiseHelper.Noise3(x / u, z / u, seed + i + (SeedOffset * 32)) * u;
            u += 2f;
        }
        return v;
    }
}
