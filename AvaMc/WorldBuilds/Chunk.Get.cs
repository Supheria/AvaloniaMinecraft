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
    
    public BlockData.Data GetBlockData(BlockWorldPosition position)
    {
        return World.GetBlockData(position);
    }

    public BlockId GetBlockId(BlockChunkPosition position)
    {
        return GetBlockId(position.ToInternal());
    }
    
    public BlockId GetBlockId(BlockWorldPosition position)
    {
        return World.GetBlockId(position);
    }

    private BlockId GetBlockId(Vector3I position)
    {
        var data = GetBlockData(position);
        return data.Id;
    }

    private LightIbgrs GetAllLight(Vector3I position)
    {
        var data = GetBlockData(position);
        return data.AllLight;
    }

    public LightIbgrs GetAllLight(BlockChunkPosition position)
    {
        return GetAllLight(position.ToInternal());
    }
    
    public LightIbgrs GetAllLight(BlockWorldPosition position)
    {
        return World.GetAllLight(position);
    }
}