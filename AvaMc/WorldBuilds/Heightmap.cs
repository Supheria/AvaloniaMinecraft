using System;
using System.Collections.Generic;
using System.Numerics;
using AvaMc.Coordinates;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

public sealed class Heightmap
{
    public const int UnknownHeight = int.MinValue;
    const int Volume = Chunk.ChunkSizeX * Chunk.ChunkSizeZ;
    public Vector2I Offset { get; }
    Memory<int> HeightData { get; }
    Memory<WorldgenData?> WorldgenData { get; }
    public bool Generated { get; set; }

    public Heightmap(Vector2I offset)
    {
        Offset = offset;
        HeightData = new int[Volume];
        var heightData = HeightData.Span;
        for (var i = 0; i < Volume; i++)
            heightData[i] = UnknownHeight;
        WorldgenData = new WorldgenData?[Volume];
    }

    private int PositionToIndex(Vector2I position)
    {
        return position.X * Chunk.ChunkSizeX + position.Y;
    }

    private int PositionToIndex(int x, int z)
    {
        return x * Chunk.ChunkSizeX + z;
    }

    public void SetHeight(BlockPosition position)
    {
        var pos = position.IntoHeightmap();
        var index = PositionToIndex(pos);
        HeightData.Span[index] = position.Height;
    }

    private int GetHeight(Vector2I position)
    {
        var index = PositionToIndex(position);
        return HeightData.Span[index];
    }

    public int GetHeight(BlockPosition position)
    {
        var pos = position.IntoHeightmap();
        return GetHeight(pos);
    }

    public int GetHeight(BlockChunkPosition position)
    {
        var pos = position.Xz();
        return GetHeight(pos);
    }

    public int GetHeight(int x, int z)
    {
        return GetHeight(new Vector2I(x, z));
    }

    // TODO: split gen-data from heightmap
    public void SetGenData(int x, int z, WorldgenData gen)
    {
        var index = PositionToIndex(x, z);
        WorldgenData.Span[index] = gen;
    }

    public WorldgenData? GetGenData(int x, int z)
    {
        var index = PositionToIndex(x, z);
        return WorldgenData.Span[index];
    }

    // int[,] Data { get; set; }

    // WorldgenData WorldgenData { get; set; }
    // bool Generated { get; set; } = true;
    //
    // public int GetData(float x, float z)
    // {
    //     return Data[(int)x, (int)z];
    // }
    //
    // public void SetData(float x, float z, float value)
    // {
    //     Data[(int)x, (int)z] = (int)value;
    // }
}
