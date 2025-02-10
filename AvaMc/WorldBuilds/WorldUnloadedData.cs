using AvaMc.Util;

namespace AvaMc.WorldBuilds;

public sealed class WorldUnloadedData
{
    public Vector3I Position { get; }
    public BlockDataService Data { get; }
    public WorldUnloadedData(Vector3I position, BlockDataService data)
    {
        Position = position;
        Data = data;
    }
}
