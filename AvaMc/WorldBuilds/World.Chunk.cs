using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvaMc.Blocks;
using AvaMc.Comparers;
using AvaMc.Coordinates;
using AvaMc.Gfx;
using AvaMc.Util;
using Hexa.NET.Utilities;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

unsafe partial class World
{
    private void LoadEmptyChunks(GL gl)
    {
        var offsets = new UnsafeList<Vector3I>();
        for (var i = 0; i < ChunksVolume; i++)
        {
            var pChunk = _chunkPointers[i];
            if (pChunk != IntPtr.Zero)
                continue;
            var offset = ChunkIndexToOffset(i);
            offsets.Add(offset);
        }
        var comparer = new ChunkDepthComparer(CenterChunkOffset, DepthOrder.Nearer);
        offsets.AsSpan().Sort(comparer);

        for (var i = 0; i < offsets.Count; i++)
        {
            var offset = offsets[i];
            var index = ChunkOffsetToIndex(offset);
            if (!Meshing.UnderThreshold())
                continue;
            _chunkPointers[index] = (IntPtr)LoadChunk(gl, offset);
            Meshing.AddOne();
        }
        offsets.Release();
    }

    private Chunk* LoadChunk(GL gl, Vector3I offset)
    {
        if (!ChunkInBounds(offset))
            throw new ArgumentOutOfRangeException(nameof(offset), offset, null);

        var pChunk = Utils.AllocT<Chunk>(1);
        *pChunk = new Chunk(gl, offset);
        pChunk->Generating = true;
        Generator.Generate(pChunk);
        foreach (var (position, blockId) in UnloadedBlockIds)
        {
            if (position.ToChunkOffset() != offset)
                continue;
            var pos = position.IntoChunk();
            pChunk->SetBlockId(pos.X, pos.Y, pos.Z, blockId);
            UnloadedBlockIds.Remove(position);
        }

        pChunk->AfterGenerate();
        pChunk->Generating = false;
        return pChunk;
    }

    public bool GetChunk(Vector3I offset, [NotNullWhen(true)] out Chunk* pChunk)
    {
        pChunk = null;
        if (!ChunkInBounds(offset))
            return false;
        var index = ChunkOffsetToIndex(offset);
        pChunk = (Chunk*)_chunkPointers[index];
        return pChunk != null;
    }

    // TODO: cache unloaded chunks' blocks
    private bool GetChunk(BlockPosition position, out Chunk* pChunk)
    {
        var offset = position.ToChunkOffset();
        return GetChunk(offset, out pChunk);
    }

    public BlockId GetBlockId(BlockPosition position)
    {
        if (!GetChunk(position, out var chunk))
            return BlockId.Air;
        var pos = position.IntoChunk();
        return chunk->GetBlockId(pos.X, pos.Y, pos.Z);
    }

    public AllLight GetAllLight(BlockPosition position)
    {
        if (!GetChunk(position, out var chunk))
            return new();
        var pos = position.IntoChunk();
        return chunk->GetAllLight(pos.X, pos.Y, pos.Z);
    }

    public TorchLight GetTorchLight(BlockPosition position)
    {
        if (!GetChunk(position, out var chunk))
            return new();
        var pos = position.IntoChunk();
        return chunk->GetTorchLight(pos.X, pos.Y, pos.Z);
    }

    public BlockData GetBlockData(BlockPosition position)
    {
        if (!GetChunk(position, out var chunk))
            return new();
        var pos = position.IntoChunk();
        return chunk->GetBlockData(pos.X, pos.Y, pos.Z);
    }

    public void SetBlockId(BlockPosition position, BlockId id)
    {
        if (!GetChunk(position, out var chunk))
            UnloadedBlockIds[position] = id;
        else
        {
            var pos = position.IntoChunk();
            chunk->SetBlockId(pos.X, pos.Y, pos.Z, id);
        }
    }

    public void SetAllLight(BlockPosition position, AllLight allLight)
    {
        if (!GetChunk(position, out var chunk))
            return;
        var pos = position.IntoChunk();
        chunk->SetAllLight(pos.X, pos.Y, pos.Z, allLight);
    }

    public void SetSunlight(BlockPosition position, int sunlight)
    {
        if (!GetChunk(position, out var chunk))
            return;
        var pos = position.IntoChunk();
        chunk->SetSunlight(pos.X, pos.Y, pos.Z, sunlight);
    }
}
