using System;
using System.Numerics;
using AvaMc.Extensions;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Coordinates;

public struct BlockPosition : IEquatable<BlockPosition>
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int Height => Y;
    public static BlockPosition Zero { get; } = new();

    public BlockPosition()
        : this(0, 0, 0) { }

    public BlockPosition(Vector3I value)
    {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
    }

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

    public Vector2I IntoHeightmap()
    {
        var x = (X % Chunk.ChunkSizeX + Chunk.ChunkSizeX) % Chunk.ChunkSizeX;
        var z = (Z % Chunk.ChunkSizeZ + Chunk.ChunkSizeZ) % Chunk.ChunkSizeZ;
        return new(x, z);
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
        var dir = direction.Vector3I;
        var x = X + dir.X;
        var y = Y + dir.Y;
        var z = Z + dir.Z;
        return new(x, y, z);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public bool Equals(BlockPosition other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is BlockPosition other && Equals(other);
    }
}
