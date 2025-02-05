using AvaMc.Blocks;

namespace AvaMc.WorldBuilds;

// - 28 bits metadata/extra
// - 4 bits sunlight intensity
// - 4 bits R light
// - 4 bits G light
// - 4 bits B light
// - 4 bits light intensity
// - 16 bits block id
public struct BlockData
{
    //TODO: - 28 bits metadata/extra
    //

    // public float SunLightIntensity { get; set; }
    // public Vector3 Color { get; set; }
    // public float LightIntensity { get; set; }
    public BlockId Id { get; private set; }
    public int SunLight {get;private set;}
    public int Light { get; private set; }
    // public uint AllLight { get; private set; }

    public BlockData()
    {
        Id = BlockId.Air;
    }

    public BlockData SetId(BlockId id)
    {
        Id = id;
        return this;
    }

    public static BlockData New(BlockId id)
    {
        var data = new BlockData() { Id = id };
        return data;
    }

    public BlockData SetLight(int light)
    {
        Light = light;
        return this;
    }
    
    public uint GetAllLight()
    {
        var allLight = (SunLight << 16) + (Light & 0xFFFF);
        if (Light != 0)
        {
            
        }
        if (allLight != 0)
        {
            
        }
        return (uint)allLight & 0xFFFFF;
    }
}
