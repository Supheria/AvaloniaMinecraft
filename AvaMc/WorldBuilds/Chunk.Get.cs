using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Gfx;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

partial class Chunk
{
    private BlockData GetBlockData(Vector3I position)
    {
        if (Data.TryGetValue(position, out var data))
            return data;
        Data[position] = new();
        return Data[position];
    }

    public BlockData.Data GetBlockData(BlockChunkPosition position)
    {
        var data = GetBlockData(position.ToInternal());
        return data.GetData();
    }

    public BlockId GetBlockId(BlockChunkPosition position)
    {
        return GetBlockId(position.ToInternal());
    }

    private BlockId GetBlockId(Vector3I position)
    {
        var data = GetBlockData(position);
        return data.Id;
    }

    public LightRgbi GetBlockLight(BlockChunkPosition position)
    {
        return GetBlockLight(position.ToInternal());
    }

    private LightRgbi GetBlockLight(Vector3I position)
    {
        var data = GetBlockData(position);
        return data.Light;
    }
}