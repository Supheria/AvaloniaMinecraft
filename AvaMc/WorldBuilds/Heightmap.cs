using System.Collections.Generic;
using System.Numerics;
using AvaMc.Coordinates;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

public sealed class Heightmap
{
    public const int UnknownHeight = int.MinValue;
    Dictionary<Vector2I, int> HeightData { get; } = [];
    Dictionary<Vector2, WorldgenData> WorldgenData { get; } = [];
    public bool Generated { get; set; }

    public void SetHeight(BlockPosition position)
    {
        var pos = position.IntoHeightmap();
        HeightData[pos] = position.Height;
    }

    private int GetHeight(Vector2I position)
    {
        if (HeightData.TryGetValue(position, out var height))
            return height;
        HeightData[position] = UnknownHeight;
        return HeightData[position];
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
        WorldgenData[new(x, z)] = gen;
    }
    
    public WorldgenData GetGenData(int x, int z)
    {
        return WorldgenData[new(x, z)];
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
