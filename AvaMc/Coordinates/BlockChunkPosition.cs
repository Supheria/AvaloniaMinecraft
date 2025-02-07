using System;
using System.Collections.Generic;
using System.Numerics;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Coordinates;

public readonly struct BlockChunkPosition : IEquatable<BlockChunkPosition>
{
    Vector3I Value { get; }
    Vector3I ChunkPosition { get; }

    public override int GetHashCode() => Value.GetHashCode();

    public BlockChunkPosition(Vector3I value, Vector3I chunkPosition)
    {
        Value = value;
        ChunkPosition = chunkPosition;
    }

    public BlockChunkPosition ToNeighbor(Direction direction)
    {
        var val = Vector3I.Add(Value, direction.Vector3I);
        return new(val, ChunkPosition);
    }

    public BlockWorldPosition ToWorld()
    {
        var val = Vector3I.Add(Value, ChunkPosition);
        return new(val);
    }

    public bool Equals(BlockChunkPosition other)
    {
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is BlockChunkPosition other && Equals(other);
    }

    public bool InChunkBounds()
    {
        return Value.X >= 0
            && Value.Y >= 0
            && Value.Z >= 0
            && Value.X < ChunkData.ChunkSizeX
            && Value.Y < ChunkData.ChunkSizeY
            && Value.Z < ChunkData.ChunkSizeZ;
    }
    
    public Vector3 ToNumerics()
    {
        return Value.ToNumerics();
    }
    
    public Vector3I ToInternal()
    {
        return Value;
    }
}
