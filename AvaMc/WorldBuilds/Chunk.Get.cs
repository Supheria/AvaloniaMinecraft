using System;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Gfx;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

partial class Chunk
{
    private static int PositionToIndex(Vector3I position)
    {
        return position.X * ChunkSizeX * ChunkSizeZ + position.Z * ChunkSizeZ + position.Y;
    }

    private BlockDataService GetBlockDataService(Vector3I position)
    {
        var index = PositionToIndex(position);
        // var data = Data.AsSpan();
        return Data[index] ?? (Data[index] = new());
    }

    public BlockData GetBlockData(Vector3I position)
    {
        var service = GetBlockDataService(position);
        return service.Data;
    }

    public BlockData GetBlockData(BlockChunkPosition position)
    {
        return GetBlockData(position.ToInternal());
    }

    public BlockId GetBlockId(BlockChunkPosition position)
    {
        return GetBlockId(position.ToInternal());
    }

    public BlockId GetBlockId(Vector3I position)
    {
        var service = GetBlockDataService(position);
        return service.BlockId;
    }

    public AllLight GetAllLight(Vector3I position)
    {
        var service = GetBlockDataService(position);
        return service.AllLight;
    }

    public AllLight GetAllLight(BlockChunkPosition position)
    {
        return GetAllLight(position.ToInternal());
    }

    public TorchLight GetTorchLight(Vector3I position)
    {
        var service = GetBlockDataService(position);
        return service.TorchLight;
    }

    public TorchLight GetTorchLight(BlockChunkPosition position)
    {
        return GetTorchLight(position.ToInternal());
    }
}
