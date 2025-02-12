using System;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Gfx;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

partial class Chunk
{
    private void SetBlockData(Vector3I position, BlockData data)
    {
        var index = PositionToIndex(position);
        Data[index] = data;
    }
    public void SetBlockId(Vector3I position, BlockId blockId)
    {
        var prev = GetBlockDataService(position);
        var changed = prev;
        changed.BlockId = blockId;
        SetBlockData(position, changed);
        OnModify(position, prev, changed);
    }

    public void SetBlockId(BlockChunkPosition position, BlockId blockId)
    {
        SetBlockId(position.ToInternal(), blockId);
    }

    public void SetBlockId(int x, int y, int z, BlockId blockId)
    {
        SetBlockId(new Vector3I(x, y, z), blockId);
    }

    public void SetAllLight(Vector3I position, AllLight allLight)
    {
        var prev = GetBlockDataService(position);
        var changed = prev;
        changed.AllLight = allLight;
        SetBlockData(position, changed);
        OnModify(position, prev, changed);
    }

    public void SetAllLight(BlockChunkPosition position, AllLight allLight)
    {
        SetAllLight(position.ToInternal(), allLight);
    }

    public void SetSunlight(Vector3I position, int sunlight)
    {
        var prev = GetBlockDataService(position);
        var changed = prev;
        changed.Sunlight = sunlight;
        SetBlockData(position, changed);
        OnModify(position, prev, changed);
    }

    public void SetSunlight(BlockChunkPosition position, int sunlight)
    {
        SetSunlight(position.ToInternal(), sunlight);
    }
}
