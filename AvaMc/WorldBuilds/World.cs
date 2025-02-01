using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvaMc.Blocks;
using AvaMc.Entities;
using AvaMc.Extensions;
using Microsoft.Xna.Framework;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

// TODO
public sealed partial class World
{
    const float ChunksSize = 16;
    public Player Player { get; set; }
    Dictionary<Vector3, Chunk> Chunks { get; set; } = [];
    Vector3 ChunksOrigin { get; set; }
    Vector3 CenterOffset { get; set; }

    public World(GL gl)
    {
        Player = new(this);
        SetCenter(gl, Vector3.Zero);
    }
    
    public void Delete(GL gl)
    {
        Player.Delete(gl);
        foreach (var chunk in Chunks.Values)
            chunk.Delete(gl);
        Chunks.Clear();
    }

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
        if (!ChunkInBounds(offset))
            return false;
        return Chunks.TryGetValue(offset, out chunk);
    }

    public void LoadChunk(GL gl, Vector3 offset)
    {
        var chunk = new Chunk(gl, this, offset);
        Generate(chunk);
        Chunks[offset] = chunk;
    }

    public void LoadEmptyChunks(GL gl)
    {
        // LoadChunk(gl, new(0, 0, 0));
        for (var x = 0; x < ChunksSize; x++)
        {
            for (var z = 0; z < ChunksSize; z++)
            {
                // if (x != 0 || z != 0)
                //     continue;
                var offset = Vector3.Add(ChunksOrigin, new(x, 0, z));
                if (!Chunks.ContainsKey(offset))
                    LoadChunk(gl, offset);
            }
        }
    }

    public void SetCenter(GL gl, Vector3 center)
    {
        var newOffset = Chunk.BlockPositionToChunkOffset(center);
        var newOrigin = Vector3.Subtract(newOffset, new(ChunksSize / 2, 0, ChunksSize / 2));
        if (ChunksOrigin == newOrigin)
            return;
        CenterOffset = newOffset;
        ChunksOrigin = newOrigin;
        foreach (var (offset, chunk) in Chunks)
        {
            if (!ChunkInBounds(offset))
            {
                chunk.Delete(gl);
                Chunks.Remove(offset);
            }
        }
        LoadEmptyChunks(gl);
    }
    
    public void Render(GL gl)
    {
        foreach (var chunk in Chunks.Values)
            chunk.Render(gl);
        Player.Render(gl);
    }
}
