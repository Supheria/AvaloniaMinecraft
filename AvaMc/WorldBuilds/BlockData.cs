using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Gfx;

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
    public readonly struct Data
    {
        public BlockId Id { get; }
        public LightRgbi Light { get; }
        public Data(BlockId id, LightRgbi light)
        {
            Id = id;
            Light = light;
        }
    }

    //TODO: - 28 bits metadata/extra
    //

    // public float SunLightIntensity { get; set; }
    // public Vector3 Color { get; set; }
    // public float LightIntensity { get; set; }
    public BlockId Id { get; set; } = BlockId.Air;
    public int SunLight { get; set; }
    public LightRgbi Light { get; set; } = new();

    public Data GetData()
    {
        return new(Id, Light);
    }

    // public uint AllLight { get; private set; }

    // public uint GetAllLight()
    // {
    //     // var allLight = (SunLight << 16) + (Light & 0xFFFF);
    //     // if (Light != 0)
    //     // {
    //     //
    //     // }
    //     // if (allLight != 0)
    //     // {
    //     //
    //     // }
    //     // return (uint)allLight & 0xFFFFF;
    // }
}
