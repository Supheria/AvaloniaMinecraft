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
        var offsets = new Vector3I[ChunksVolume].AsSpan();
        for (var i = 0; i < ChunksVolume; i++)
        {
            var offset = ChunkIndexToOffset(i);
            offsets[i] = offset;
        }
        var comparer = new ChunkDepthComparer(CenterChunkOffset, DepthOrder.Nearer);
        offsets.Sort(comparer);

        var chunks = Chunks.AsSpan();
        for (var i = 0; i < ChunksVolume; i++)
        {
            var offset = offsets[i];
            var index = ChunkOffsetToIndex(offset);
            if (chunks[index] is null && Meshing.UnderThreshold())
            {
                chunks[index] = LoadChunk(gl, offset);
                Meshing.AddOne();
            }
        }
    }

    private Chunk LoadChunk(GL gl, Vector3I offset)
    {
        if (!ChunkInBounds(offset))
            throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
        var chunk = new Chunk(gl, this, offset);
        chunk.Generating = true;
        Generator.Generate(chunk);
        foreach (var (position, blockId) in UnloadedBlockIds)
        {
            if (position.ToChunkOffset() != offset)
                continue;
            var pos = position.IntoChunk();
            chunk.SetBlockId(pos.X, pos.Y, pos.Z, blockId);
            UnloadedBlockIds.Remove(position);
        }
        chunk.AfterGenerate();
        chunk.Generating = false;
        return chunk;
    }

    public Chunk? GetChunk(Vector3I offset)
    {
        if (!ChunkInBounds(offset))
            return null;
        // var chunks = Chunks.AsSpan();
        var index = ChunkOffsetToIndex(offset);
        return Chunks[index];
    }

    // TODO: cache unloaded chunks' blocks
    private Chunk? GetChunk(BlockPosition position)
    {
        var offset = position.ToChunkOffset();
        return GetChunk(offset);
    }

    public BlockId GetBlockId(BlockPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return BlockId.Air;
        var pos = position.IntoChunk();
        return chunk.GetBlockId(pos.X, pos.Y, pos.Z);
    }

    public AllLight GetAllLight(BlockPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return new();
        var pos = position.IntoChunk();
        return chunk.GetAllLight(pos.X, pos.Y, pos.Z);
    }

    public TorchLight GetTorchLight(BlockPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return new();
        var pos = position.IntoChunk();
        return chunk.GetTorchLight(pos.X, pos.Y, pos.Z);
    }

    public BlockData GetBlockData(BlockPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return new();
        var pos = position.IntoChunk();
        return chunk.GetBlockData(pos.X, pos.Y, pos.Z);
    }

    public void SetBlockId(BlockPosition position, BlockId id)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            UnloadedBlockIds[position] = id;
        else
        {
            var pos = position.IntoChunk();
            chunk.SetBlockId(pos.X, pos.Y, pos.Z, id);
        }
    }

    public void SetAllLight(BlockPosition position, AllLight allLight)
    {
        var chunk = GetChunk(position);
        var pos = position.IntoChunk();
        chunk?.SetAllLight(pos.X, pos.Y, pos.Z, allLight);
    }

    public void SetSunlight(BlockPosition position, int sunlight)
    {
        var chunk = GetChunk(position);
        var pos = position.IntoChunk();
        chunk?.SetSunlight(pos.X, pos.Y, pos.Z, sunlight);
    }
}
