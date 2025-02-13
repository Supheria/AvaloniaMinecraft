using System;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Gfx;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

partial class Chunk
{
    private void SetBlockData(int x, int y, int z, BlockData data)
    {
        var index = PositionToIndex(x, y, z);
        var prev = Data[index];
        Data[index] = data;
        OnModify(x, y, z, prev, data);
    }

    public void SetBlockId(int x, int y, int z, BlockId blockId)
    {
        var data = GetBlockData(x, y, z);
        data.BlockId = blockId;
        SetBlockData(x, y, z, data);
    }

    public void SetBlockId(BlockChunkPosition position, BlockId blockId)
    {
        SetBlockId(position.X, position.Y, position.Z, blockId);
    }

    public void SetAllLight(int x, int y, int z, AllLight allLight)
    {
        var data = GetBlockData(x, y, z);
        data.AllLight = allLight;
        SetBlockData(x, y, z, data);
    }

    public void SetSunlight(int x, int y, int z, int sunlight)
    {
        var data = GetBlockData(x, y, z);
        data.Sunlight = sunlight;
        SetBlockData(x, y, z, data);
    }

    public void SetSunlight(BlockChunkPosition position, int sunlight)
    {
        SetSunlight(position.X, position.Y, position.Z, sunlight);
    }
}
