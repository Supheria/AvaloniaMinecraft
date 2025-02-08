using System;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Gfx;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

partial class Chunk
{
    private void SetBlockId(Vector3I position, BlockId id)
    {
        var data = GetBlockData(position);
        data.SetId(id, out var prev, out var changed);
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

    public void SetBlockId(BlockWorldPosition position, BlockId id)
    {
        World.SetBlockId(position, id);
    }

    private void SetAllLight(Vector3I position, LightIbgrs light)
    {
        var data = GetBlockData(position);
        data.SetAllLight(light, out var prev, out var changed);
        OnModify(position, prev, changed);
    }

    public void SetAllLight(BlockChunkPosition position, LightIbgrs light)
    {
        SetAllLight(position.ToInternal(), light);
    }

    private void SetSunlight(Vector3I position, int sunlight)
    {
        var data = GetBlockData(position);
        data.SetSunlight(sunlight, out var prev, out var changed);
        OnModify(position, prev, changed);
    }

    public void SetSunlight(BlockChunkPosition position, int sunlight)
    {
        SetSunlight(position.ToInternal(), sunlight);
    }
    
    public void SetSunlight(BlockWorldPosition position, int sunlight)
    {
        World.SetSunlight(position, sunlight);
    }
}
