using AvaMc.Blocks;

namespace AvaMc.WorldBuilds;

// - 28 bits metadata/extra
// - 4 bits sunlight intensity
// - 4 bits R light
// - 4 bits G light
// - 4 bits B light
// - 4 bits light intensity
// - 16 bits block id
public sealed class BlockData
{
    //TODO: - 28 bits metadata/extra
    //

    // public float SunLightIntensity { get; set; }
    // public Vector3 Color { get; set; }
    // public float LightIntensity { get; set; }
    public BlockId BlockId { get; set; }
}
