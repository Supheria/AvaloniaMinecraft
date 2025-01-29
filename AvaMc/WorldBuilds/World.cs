using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvaMc.Blocks;
using Microsoft.Xna.Framework;

namespace AvaMc.WorldBuilds;

// TODO
public sealed class World
{
    const int ChunksSize = 16;
    List<Chunk> Chunks { get; set; } = [];
    Vector3 ChunksOrigin { get; set; }
    Heightmap[,] Heightmaps { get; set; } = new Heightmap[ChunksSize, ChunksSize];

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

    public int ChunkIndex(Vector3 offset)
    {
        var p = Vector3.Subtract(offset, ChunksOrigin);
        var index = (p.X * ChunksSize * ChunksSize) + (p.Z * ChunksSize);
        return (int)index;
    }

    public bool GetChunk(Vector3 offset, [NotNullWhen(true)] out Chunk? chunk)
    {
        chunk = null;
        if (ChunkInBounds(offset))
            return false;
        var index = ChunkIndex(offset);
        chunk = Chunks[index];
        return true;
    }

    public int HeightmapIndex(Vector2 offset)
    {
        var p = Vector2.Subtract(offset, new(ChunksOrigin.X, ChunksOrigin.Z));
        return (int)(p.X * ChunksSize + p.Y);
    }

    public static Heightmap GetHeightmap(Chunk chunk)
    {
        return chunk.World.Heightmaps[(int)chunk.Offset.X, (int)chunk.Offset.Z];
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
