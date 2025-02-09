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
        var val = IntoWorld();
        return val.ToNeighbor(direction);
    }

    public BlockPosition IntoWorld()
    {
        var val = Vector3I.Add(Value, ChunkPosition);
        return new(val);
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
