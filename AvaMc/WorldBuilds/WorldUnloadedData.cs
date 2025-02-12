using AvaMc.Util;

namespace AvaMc.WorldBuilds;

public sealed class WorldUnloadedData
{
    public Vector3I Position { get; }
    public BlockData Data { get; }
    public WorldUnloadedData(Vector3I position, BlockData data)
    {
        Position = position;
        Data = data;
    }
}
