using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvaMc.Blocks;
using AvaMc.Comparers;
using AvaMc.Coordinates;
using AvaMc.Gfx;
using AvaMc.Util;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

partial class World
{
    private void LoadEmptyChunks(GL gl)
    {
        var offsets = new List<Vector3I>();
        for (var x = 0; x < ChunksMagnitude; x++)
        {
            for (var y = 0; y < ChunksMagnitude; y++)
            {
                for (var z = 0; z < ChunksMagnitude; z++)
                {
                    var offset = Vector3I.Add(ChunksOrigin, new(x, y, z));
                    offsets.Add(offset);
                }
            }
        }
        var comparer = new ChunkDepthComparer(CenterChunkOffset, DepthOrder.Nearer);
        offsets.Sort(comparer);

        foreach (var offset in offsets)
        {
            if (!Chunks.ContainsKey(offset) && Meshing.UnderThreshold())
            {
                LoadChunk(gl, offset);
                Meshing.AddOne();
            }
        }
    }

    private void LoadChunk(GL gl, Vector3I offset)
    {
        if (!InBounds(offset))
            throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
        var chunk = new Chunk(gl, this, offset);
        Chunks[offset] = chunk;
        chunk.Generating = true;
        Generator.Generate(chunk);
        foreach (var (position, blockId) in UnloadedBlockIds)
        {
            if (position.ToChunkOffset() != offset)
                continue;
            var cPos = chunk.CreatePosition(position);
            chunk.SetBlockId(cPos, blockId);
            UnloadedBlockIds.Remove(position);
        }
        chunk.Generating = false;
        chunk.AfterGenerate();
    }

    public bool GetChunk(ChunkOffset offset, [NotNullWhen(true)] out Chunk? chunk)
    {
        return GetChunk(offset.ToInernal(), out chunk);
    }

    private bool GetChunk(Vector3I offset, [NotNullWhen(true)] out Chunk? chunk)
    {
        chunk = null;
        if (!InBounds(offset))
            return false;
        return Chunks.TryGetValue(offset, out chunk);
    }

    // TODO: cache unloaded chunks' blocks
    private Chunk? GetChunk(BlockPosition position)
    {
        var offset = position.ToChunkOffset();
        if (GetChunk(offset, out var chunk))
            return chunk;
        return null;
    }

    public BlockId GetBlockId(BlockPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return BlockId.Air;
        return chunk.GetBlockId(position.IntoChunk());
    }

    public AllLight GetAllLight(BlockPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return new();
        return chunk.GetAllLight(position.IntoChunk());
    }
    
    public TorchLight GetTorchLight(BlockPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return new();
        return chunk.GetTorchLight(position.IntoChunk());
    }

    public BlockData GetBlockData(BlockPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return new();
        return chunk.GetBlockData(position.IntoChunk());
    }

    public void SetBlockId(BlockPosition position, BlockId id)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            UnloadedBlockIds[position] = id;
        else
            chunk.SetBlockId(position.IntoChunk(), id);
    }

    public void SetAllLight(BlockPosition position, AllLight allLight)
    {
        var chunk = GetChunk(position);
        chunk?.SetAllLight(position.IntoChunk(), allLight);
    }

    public void SetSunlight(BlockPosition position, int sunlight)
    {
        var chunk = GetChunk(position);
        chunk?.SetSunlight(position.IntoChunk(), sunlight);
    }
}
