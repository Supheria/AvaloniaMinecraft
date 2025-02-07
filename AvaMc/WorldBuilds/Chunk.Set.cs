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
        data.SetId(id, out var old, out var @new);
        OnModify(position, old, @new);
    }

    public void SetBlockId(BlockChunkPosition position, BlockId id)
    {
        SetBlockId(position.ToInternal(), id);
    }

    public void SetBlockId(int x, int y, int z, BlockId id)
    {
        SetBlockId(new Vector3I(x, y, z), id);
    }

    public bool SetBlockId(BlockWorldPosition position, BlockId id)
    {
        var offset = position.ToChunkOffset();
        if (offset != Offset.ToInernal())
            return false;
        SetBlockId(position.ToChunk(), id);
        return true;
    }

    private void SetBlockLight(Vector3I position, LightRgbi light)
    {
        var data = GetBlockData(position);
        data.SetLight(light, out var old, out var @new);
        OnModify(position, old, @new);
    }

    public void SetBlockLight(BlockChunkPosition position, LightRgbi light)
    {
        SetBlockLight(position.ToInternal(), light);
    }
}