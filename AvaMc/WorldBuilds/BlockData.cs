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
        public LightIbgrs AllLight { get; }
        public Data(BlockId id, LightIbgrs allLight)
        {
            Id = id;
            AllLight = allLight;
        }
    }
    public BlockId Id { get; private set; } = BlockId.Air;
    LightIbgrs _allLight  = new();
    public LightIbgrs AllLight => _allLight;

    public Data GetData()
    {
        return new(Id, _allLight);
    }
    
    public void SetId(BlockId id, out Data prev, out Data changed)
    {
        prev = GetData();
        Id = id;
        changed = GetData();
    }
    
    public void SetAllLight(LightIbgrs allLight, out Data prev, out Data changed)
    {
        prev = GetData();
        _allLight = allLight;
        changed = GetData();
    }
    
    public void SetSunlight(int sunlight, out Data prev, out Data changed)
    {
        prev = GetData();
        _allLight.Sunlight = sunlight;
        changed = GetData();
    }
}
