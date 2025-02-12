using System;
using System.Collections.Generic;
using System.Numerics;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Coordinates;

public readonly struct BlockChunkPosition
{
    Vector3I Value { get; }
    Vector3I ChunkPosition { get; }

    public override int GetHashCode() => Value.GetHashCode();

    public BlockChunkPosition(Vector3I value, Vector3I chunkPosition)
    {
        Value = value;
        ChunkPosition = chunkPosition;
    }

    public BlockPosition ToNeighbor(Direction direction)
    {
        var x = Value.X + direction.X + ChunkPosition.X;
        var y = Value.Y + direction.Y + ChunkPosition.Y;
        var z = Value.Z + direction.Z + ChunkPosition.Z;
        return new(x, y, z);
    }

    public BlockPosition IntoWorld()
    {
        var x = Value.X + ChunkPosition.X;
        var y = Value.Y + ChunkPosition.Y;
        var z = Value.Z + ChunkPosition.Z;
        return new(x, y, z);
    }

    public Vector3 ToNumerics()
    {
        return Value.ToNumerics();
    }

    public Vector3I ToInternal()
    {
        return Value;
    }

    public Vector2I Xz()
    {
        return new(Value.X, Value.Z);
    }
}
