using System;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Gfx;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

partial class Chunk
{
    public void SetBlockId(Vector3I position, BlockId id)
    {
        var service = GetBlockDataService(position);
        service.SetBlockId(id, out var prev, out var changed);
        OnModify(position, prev, changed);
    }

    public void SetBlockId(BlockChunkPosition position, BlockId id)
    {
        SetBlockId(position.ToInternal(), id);
    }

    public void SetBlockId(int x, int y, int z, BlockId id)
    {
        SetBlockId(new Vector3I(x, y, z), id);
    }

    public void SetAllLight(Vector3I position, AllLight allLight)
    {
        var service = GetBlockDataService(position);
        service.SetAllLight(allLight, out var prev, out var changed);
        OnModify(position, prev, changed);
    }

    public void SetAllLight(BlockChunkPosition position, AllLight allLight)
    {
        SetAllLight(position.ToInternal(), allLight);
    }

    public void SetSunlight(Vector3I position, int sunlight)
    {
        var service = GetBlockDataService(position);
        service.SetSunlight(sunlight, out var prev, out var changed);
        OnModify(position, prev, changed);
    }

    public void SetSunlight(BlockChunkPosition position, int sunlight)
    {
        SetSunlight(position.ToInternal(), sunlight);
    }
}
