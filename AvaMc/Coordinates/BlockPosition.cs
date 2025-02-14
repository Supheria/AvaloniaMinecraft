using System;
using System.Numerics;
using AvaMc.Extensions;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Coordinates;

public struct BlockPosition
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int Height => Y;
    public static BlockPosition Zero { get; } = new();

    public BlockPosition()
        : this(0, 0, 0) { }

    public BlockPosition(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public BlockPosition(Vector3 position)
        : this(
            (int)MathF.Floor(position.X),
            (int)MathF.Floor(position.Y),
            (int)MathF.Floor(position.Z)
        ) { }

    public Vector3I ToChunkOffset()
    {
        return new(
            (int)MathF.Floor(X / (float)Chunk.ChunkSizeX),
            (int)MathF.Floor(Y / (float)Chunk.ChunkSizeY),
            (int)MathF.Floor(Z / (float)Chunk.ChunkSizeZ)
        );
    }

    public (int X, int Z) IntoHeightmap()
    {
        var x = (X % Chunk.ChunkSizeX + Chunk.ChunkSizeX) % Chunk.ChunkSizeX;
        var z = (Z % Chunk.ChunkSizeZ + Chunk.ChunkSizeZ) % Chunk.ChunkSizeZ;
        return (x, z);
    }

    public Vector3I IntoChunk()
    {
        var x = (X % Chunk.ChunkSizeX + Chunk.ChunkSizeX) % Chunk.ChunkSizeX;
        var y = (Y % Chunk.ChunkSizeY + Chunk.ChunkSizeY) % Chunk.ChunkSizeY;
        var z = (Z % Chunk.ChunkSizeZ + Chunk.ChunkSizeZ) % Chunk.ChunkSizeZ;
        return new(x, y, z);
    }

    public BlockPosition ToNeighbor(Direction direction)
    {
        var x = X + direction.X;
        var y = Y + direction.Y;
        var z = Z + direction.Z;
        return new(x, y, z);
    }
}
