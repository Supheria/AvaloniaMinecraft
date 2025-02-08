using System.Collections.Generic;
using System.Numerics;
using AvaMc.Coordinates;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

public sealed class Heightmap
{
    public const int UnknownHeight = int.MinValue;
    Dictionary<Vector2I, int> Data { get; } = [];

    public void Set(BlockWorldPosition position)
    {
        var pos = position.ToHeightmap();
        Data[pos] = position.Height;
    }
    
    private int Get(Vector2I position)
    {
        if (Data.TryGetValue(position, out var height))
            return height;
        Data[position] = UnknownHeight;
        return Data[position];
    }

    public int Get(BlockWorldPosition position)
    {
        var pos = position.ToHeightmap();
        return Get(pos);
    }

    public int Get(BlockChunkPosition position)
    {
        var pos = position.Xz();
        return Get(pos);
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
