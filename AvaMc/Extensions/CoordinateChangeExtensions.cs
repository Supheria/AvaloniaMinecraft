using System;
using System.Numerics;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Extensions;

public static class CoordinateChangeExtensions
{

    public static Vector3I BlockPosWorldToChunk(this Vector3I pos)
    {
        return pos.Mod(ChunkData.ChunkSize).Add(ChunkData.ChunkSize).Mod(ChunkData.ChunkSize);
    }

    public static Vector3I WorldBlockPosToChunkOffset(this Vector3I pos)
    {
        return new(
            (int)MathF.Floor(pos.X / (float)ChunkData.ChunkSizeX),
            (int)MathF.Floor(pos.Y / (float)ChunkData.ChunkSizeY),
            (int)MathF.Floor(pos.Z / (float)ChunkData.ChunkSizeZ)
        );
    }

    public static Vector3I CameraPosToBlockPos(this Vector3 pos)
    {
        return new((int)MathF.Floor(pos.X), (int)MathF.Floor(pos.Y), (int)MathF.Floor(pos.Z));
    }
}