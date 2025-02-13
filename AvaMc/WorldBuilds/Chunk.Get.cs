using System;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Gfx;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

partial class Chunk
{
    public BlockData GetBlockData(int x, int y, int z)
    {
        var index = PositionToIndex(x, y, z);
        // var data = Data.AsSpan();
        return Data[index];
    }

    public BlockId GetBlockId(int x, int y, int z)
    {
        var data = GetBlockData(x, y, z);
        return data.BlockId;
    }

    public BlockId GetBlockId(BlockChunkPosition position)
    {
        return GetBlockId(position.X, position.Y, position.Z);
    }

    public AllLight GetAllLight(int x, int y, int z)
    {
        var data = GetBlockData(x, y, z);
        return data.AllLight;
    }

    public TorchLight GetTorchLight(int x, int y, int z)
    {
        var data = GetBlockData(x, y, z);
        return data.TorchLight;
    }
}
