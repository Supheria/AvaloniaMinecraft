using System;
using System.Collections.Generic;
using System.Numerics;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Coordinates;

public readonly struct BlockChunkPosition
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    int ChunkX { get; }
    int ChunkY { get; }
    int ChunkZ { get; }

    public BlockChunkPosition(int x, int y, int z, int chunkX, int chunkY, int chunkZ)
    {
        X = x;
        Y = y;
        Z = z;
        ChunkX = chunkX;
        ChunkY = chunkY;
        ChunkZ = chunkZ;
    }

    public BlockPosition ToNeighbor(Direction direction)
    {
        var x = X + ChunkX + direction.X;
        var y = Y + ChunkY + direction.Y;
        var z = Z + ChunkZ + direction.Z;
        return new(x, y, z);
    }

    public BlockPosition IntoWorld()
    {
        var x = X + ChunkX;
        var y = Y + ChunkY;
        var z = Z + ChunkZ;
        return new(x, y, z);
    }
}