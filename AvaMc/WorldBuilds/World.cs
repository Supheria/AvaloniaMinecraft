using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvaMc.Blocks;
using AvaMc.Extensions;
using Microsoft.Xna.Framework;

namespace AvaMc.WorldBuilds;

// TODO
public sealed class World
{
    const int ChunksSize = 16;
    const int HeightmapUnknown = int.MinValue;
    Dictionary<Vector3, Chunk> Chunks { get; set; } = [];
    Vector3 ChunksOrigin { get; set; }
    Dictionary<Vector2, Heightmap> Heightmaps { get; set; } = [];

    public bool ChunkInBounds(Vector3 offset)
    {
        var p = Vector3.Subtract(offset, ChunksOrigin);
        return p.X >= 0
            && p.Y >= 0
            && p.Z >= 0
            && p.X < ChunksSize
            && p.Y < ChunksSize
            && p.Z < ChunksSize;
    }

    public bool GetChunk(Vector3 offset, [NotNullWhen(true)] out Chunk? chunk)
    {
        chunk = null;
        if (ChunkInBounds(offset))
            return false;
        chunk = Chunks[offset];
        return true;
    }
    
    public Vector2 PosToHeightmapPos(Vector2 pos)
    {
        var xz = Chunk.ChunkSize.Xz();
        return pos.Mod(xz).Add(xz).Mod(xz);
    }

    public bool HeightmapInBounds(Vector2 offset)
    {
        var p = Vector2.Subtract(offset, ChunksOrigin.Xz());
        return p.X >= 0 && p.Y >= 0 && p.X < ChunksSize && p.Y < ChunksSize;
    }

    public static Heightmap GetHeightmap(Chunk chunk)
    {
        return chunk.World.Heightmaps[chunk.Offset.Xz()];
    }

    public int HeightmapGet(Vector2 p)
    {
        var offset = Chunk.BlockPositionToChunkOffset(p);
        if (HeightmapInBounds(offset))
        {
            var heightmap = Heightmaps[offset];
            var pos = PosToHeightmapPos(p);
            return heightmap.GetData(pos.X, pos.Y);
        }
        return HeightmapUnknown;
    }

    public static void HeightMapRecaculate(Chunk chunk)
    {
        var heightmap = GetHeightmap(chunk);
        Vector3 posC = new();
        Vector3 posW = new();
        for (var x = 0; x < Chunk.ChunkSizeX; x++)
        {
            for (var z = 0; z < Chunk.ChunkSizeZ; z++)
            {
                posC.X = x;
                posC.Z = z;
                posW.X = x + chunk.Position.X;
                posW.Z = z + chunk.Position.Y;
                var h = heightmap.GetData(x, z);

                if (h > chunk.Position.Y + Chunk.ChunkSizeY - 1)
                    continue;
                for (var y = Chunk.ChunkSizeY - 1; y >= 0; y--)
                {
                    posC.Y = y;
                    posW.Y = y + chunk.Position.Y;
                    var blockId = chunk.GetBlockData(posC).BlockId;
                    if (Block.Blocks[blockId].Transparent)
                        continue;
                    heightmap.SetData(posC.X, posC.Z, posW.Y);
                }
            }
        }
    }
}
