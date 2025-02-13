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
    int[] HeightData { get; }
    WorldgenData?[] WorldgenData { get; } = [];
    public bool Generated { get; set; }

    public Heightmap(Vector2I offset)
    {
        Offset = offset;
        HeightData = new int[Volume];
        var heightData = HeightData.AsSpan();
        for (var i = 0; i < Volume; i++)
            heightData[i] = UnknownHeight;
        WorldgenData = new WorldgenData?[Volume];
    }

    private int PositionToIndex(int x, int z)
    {
        return x * Chunk.ChunkSizeX + z;
    }

    public void SetHeight(BlockPosition position)
    {
        var pos = position.IntoHeightmap();
        var index = PositionToIndex(pos.X, pos.Z);
        HeightData[index] = position.Height;
    }

    public int GetHeight(int x, int z)
    {
        var index = PositionToIndex(x, z);
        return HeightData[index];
    }

    public int GetHeight(BlockPosition position)
    {
        var pos = position.IntoHeightmap();
        return GetHeight(pos.X, pos.Z);
    }

    public int GetHeight(BlockChunkPosition position)
    {
        return GetHeight(position.X, position.Z);
    }

    // TODO: split gen-data from heightmap
    public void SetGenData(int x, int z, WorldgenData gen)
    {
        var index = PositionToIndex(x, z);
        WorldgenData[index] = gen;
    }

    public WorldgenData? GetGenData(int x, int z)
    {
        var index = PositionToIndex(x, z);
        return WorldgenData[index];
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
