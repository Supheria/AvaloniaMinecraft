using System;
using System.Collections.Generic;
using AvaMc.Extensions;
using Microsoft.Xna.Framework;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public class Chunk
{
    public const int ChunkSizeX = 32;
    public const int ChunkSizeY = 32;
    public const int ChunkSizeZ = 32;
    public static Vector3 ChunkSize { get; } = new(ChunkSizeX, ChunkSizeY, ChunkSizeZ);
    public World World { get; set; }
    public Vector3 Offset { get; set; }
    public Vector3 Position { get; set; }
    Dictionary<Vector3, BlockData> Data { get; set; } = [];
    int BlockCount { get; set; }

    // if true, this chunk contains no blocks
    bool Empty { get; set; } = true;

    // if true, this chunk is generating
    bool Generating { get; set; }
    ChunkMesh Mesh { get; set; }

    public Chunk(GL gl, World world, Vector3 offset)
    {
        World = world;
        Offset = offset;
        Position = Vector3.Multiply(offset, ChunkSize);
        Mesh = new ChunkMesh(gl, this);
    }

    public BlockData GetBlockData(Vector3 pos)
    {
        return Data[pos];
    }

    public static bool InBounds(Vector3 pos)
    {
        return pos is { X: >= 0, Y: >= 0 }
            && pos.Z >= 0
            && pos.X < ChunkSizeX
            && pos.Y < ChunkSizeY
            && pos.Z < ChunkSizeZ;
    }

    public static bool OnBounds(Vector3 pos)
    {
        return pos.X is 0
            || pos.Y is 0
            || pos.Z is 0
            || pos.X is ChunkSizeX - 1
            || pos.Y is ChunkSizeY - 1
            || pos.Z is ChunkSizeZ - 1;
    }

    public static Vector3 BlockPositionToChunkOffset(Vector3 pos)
    {
        return Vector3.Divide(pos, ChunkSize);
    }

    public static Vector2 BlockPositionToChunkOffset(Vector2 pos)
    {
        return Vector2.Divide(pos, ChunkSize.Xz());
    }

    public void Delete(GL gl)
    {
        Mesh.Delete(gl);
    }

    public List<Chunk> GetBorderingChunks(Vector3 position)
    {
        var chunks = new List<Chunk>(6);
        if (position.X is 0)
        {
            if (World.GetChunk(Vector3.Add(Offset, new(-1, 0, 0)), out var chunk))
                chunks.Add(chunk);
        }
        if (position.Y is 0)
        {
            if (World.GetChunk(Vector3.Add(Offset, new(0, -1, 0)), out var chunk))
                chunks.Add(chunk);
        }
        if (position.Z is 0)
        {
            if (World.GetChunk(Vector3.Add(Offset, new(0, 0, -1)), out var chunk))
                chunks.Add(chunk);
        }
        if ((int)position.X == ChunkSizeX - 1)
        {
            if (World.GetChunk(Vector3.Add(Offset, new(1, 0, 0)), out var chunk))
                chunks.Add(chunk);
        }
        if ((int)Position.Y == ChunkSizeY - 1)
        {
            if (World.GetChunk(Vector3.Add(Offset, new(0, 1, 0)), out var chunk))
                chunks.Add(chunk);
        }
        if ((int)Position.Z == ChunkSizeZ - 1)
        {
            if (World.GetChunk(Vector3.Add(Offset, new(0, 0, 1)), out var chunk))
                chunks.Add(chunk);
        }
        return chunks;
    }

    /// <summary>
    /// MUST be run once a chunk has completed generating
    /// </summary>
    public void AfterGenerate()
    {
        World.HeightMapRecaculate(this);
    }
}
