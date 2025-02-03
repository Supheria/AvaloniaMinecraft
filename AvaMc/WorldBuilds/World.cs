using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvaMc.Blocks;
using AvaMc.Comparers;
using AvaMc.Entities;
using AvaMc.Extensions;
using AvaMc.Util;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

// TODO
public sealed partial class World
{
    // const int ChunksSize = 16;

    const int ChunksSize = 8;
    public Player Player { get; set; }
    Dictionary<Vector3I, Chunk> Chunks { get; set; } = [];
    Vector3I ChunksOrigin { get; set; }
    Vector3I CenterOffset { get; set; }
    public Threshold Load { get; } = new(2);
    public Threshold Mesh { get; } = new(2);

    public World(GL gl)
    {
        Player = new(this);
        SetCenter(gl, Vector3I.Zero);
    }

    public void Delete(GL gl)
    {
        Player.Delete(gl);
        foreach (var chunk in Chunks.Values)
            chunk.Delete(gl);
        Chunks.Clear();
    }

    public bool ChunkInBounds(Vector3I offset)
    {
        var p = Vector3I.Subtract(offset, ChunksOrigin);
        return p.X >= 0
            && p.Y >= 0
            && p.Z >= 0
            && p.X < ChunksSize
            && p.Y < ChunksSize
            && p.Z < ChunksSize;
    }

    public bool GetChunk(Vector3I offset, [NotNullWhen(true)] out Chunk? chunk)
    {
        chunk = null;
        if (!ChunkInBounds(offset))
            return false;
        return Chunks.TryGetValue(offset, out chunk);
    }

    public void LoadChunk(GL gl, Vector3I offset)
    {
        var chunk = new Chunk(gl, this, offset);
        Generate(chunk);
        Chunks[offset] = chunk;
    }

    public BlockData GetBlockData(Vector3I position)
    {
        var offset = position.WorldBlockPosToChunkOffset();
        if (GetChunk(offset, out var chunk))
        {
            var pos = position.BlockPosWorldToChunk();
            if (chunk.GetBlockData(pos, out var data))
                return data;
        }
        return new() { BlockId = BlockId.Air };
    }

    public void SetBlockData(Vector3I position, BlockData data)
    {
        var offset = position.WorldBlockPosToChunkOffset();
        if (!GetChunk(offset, out var chunk))
            return;
        var pos = position.BlockPosWorldToChunk();
        chunk.SetData(pos, data);
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
                if (!Load.UnderThreshold())
                    break;
                var offset = Vector3I.Add(ChunksOrigin, new(x, 0, z));
                if (!Chunks.ContainsKey(offset))
                {
                    LoadChunk(gl, offset);
                    Load.AddOne();
                }
            }
        }
    }

    public void SetCenter(GL gl, Vector3I center)
    {
        var newOffset = center.WorldBlockPosToChunkOffset();
        var newOrigin = Vector3I.Subtract(
            newOffset,
            new((ChunksSize / 2) - 1, 0, (ChunksSize / 2) - 1)
        );
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
        // LoadEmptyChunks(gl);
    }

    public void Render(GL gl)
    {
        var offsets = SortChunksByOffset(DepthOrder.Farther);
        foreach (var offset in offsets)
        {
            if (!GetChunk(offset, out var chunk))
                continue;
            chunk.Render(gl);
            chunk.RenderTransparent(gl);
        }
        Player.Render(gl);
    }

    public void Update(GL gl)
    {
        Load.Reset();
        Mesh.Reset();
        LoadEmptyChunks(gl);
        foreach (var chunk in Chunks.Values)
            chunk.Update();
        Player.Update();
    }

    public void Tick()
    {
        foreach (var chunk in Chunks.Values)
            chunk.Tick();
        Player.Tick();
    }

    private Vector3I[] SortChunksByOffset(DepthOrder order)
    {
        var offsets = new List<Vector3I>();
        foreach (var chunk in Chunks.Values)
            offsets.Add(chunk.Offset);
        var comparer = new ChunkDepthComparer(CenterOffset, order);
        offsets.Sort(comparer);
        return offsets.ToArray();
    }
}
