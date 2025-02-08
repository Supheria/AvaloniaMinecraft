using System;
using System.Numerics;
using AvaMc.Extensions;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Coordinates;

public struct BlockWorldPosition
{
    Vector3I _value;
    public int X
    {
        get => _value.X;
        set => _value.X = value;
    }
    public int Y
    {
        get => _value.Y;
        set => _value.Y = value;
    }
    public int Z
    {
        get => _value.Z;
        set => _value.Z = value;
    }
    public int Height => Y;
    public static BlockWorldPosition Zero { get; } = new();

    public BlockWorldPosition()
    {
        _value = Vector3I.Zero;
    }

    public BlockWorldPosition(Vector3I value)
    {
        _value = value;
    }

    public BlockWorldPosition(int x, int y, int z)
    {
        _value = new(x, y, z);
    }

    public BlockWorldPosition(Vector3 position)
    {
        _value = new Vector3I(
            (int)MathF.Floor(position.X),
            (int)MathF.Floor(position.Y),
            (int)MathF.Floor(position.Z)
        );
    }

    public BlockWorldPosition(BlockHeightmapPosition position, int height)
    {
        _value = new Vector3I(position.X, height, position.Z);
    }

    public Vector3I ToChunkOffset()
    {
        return new(
            (int)MathF.Floor(_value.X / (float)ChunkData.ChunkSizeX),
            (int)MathF.Floor(_value.Y / (float)ChunkData.ChunkSizeY),
            (int)MathF.Floor(_value.Z / (float)ChunkData.ChunkSizeZ)
        );
    }

    public Vector2I ToChunkOffsetXz()
    {
        return new(
            (int)MathF.Floor(_value.X / (float)ChunkData.ChunkSizeX),
            (int)MathF.Floor(_value.Z / (float)ChunkData.ChunkSizeZ)
        );
    }

    public Vector2I ToHeightmap()
    {
        var val = new Vector2I(_value.X, _value.Z);
        return val.Mod(ChunkData.ChunkSizeXz).Add(ChunkData.ChunkSizeXz).Mod(ChunkData.ChunkSizeXz);
    }

    public Vector3I ToChunk()
    {
        return _value.Mod(ChunkData.ChunkSize).Add(ChunkData.ChunkSize).Mod(ChunkData.ChunkSize);
    }

    public BlockWorldPosition ToNeighbor(Direction direction)
    {
        var val = Vector3I.Add(_value, direction.Vector3I);
        return new(val);
    }
}
