using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvaMc.Blocks;
using AvaMc.Comparers;
using AvaMc.Entities;
using AvaMc.Extensions;
using AvaMc.Gfx;
using AvaMc.Util;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

// TODO
public sealed class World
{
    // const int ChunksSize = 28;

    const int ChunksSize = 8;
    public Player Player { get; set; }
    Dictionary<Vector3I, Chunk> Chunks { get; set; } = [];
    Vector3I ChunksOrigin { get; set; }
    Vector3I CenterOffset { get; set; }
    public Threshold Loading { get; } = new(1);
    public Threshold Meshing { get; } = new(8);

    // TODO: use dictionary
    public Dictionary<Vector3I, BlockId> UnloadedBlockIds { get; } = [];
    WorldGenerator Generator { get; } = new(2);

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
        Generator.Generate(chunk);
        Chunks[offset] = chunk;
    }

    // TODO: cache unloaded chunks' blocks
    private Chunk? GetChunk(Vector3I blockPos)
    {
        var offset = blockPos.BlockPosToChunkOffset();
        if (GetChunk(offset, out var chunk))
            return chunk;
        return null;
    }

    public BlockId GetBlockId(Vector3I blockPos)
    {
        var chunk = GetChunk(blockPos);
        blockPos = blockPos.BlockPosWorldToChunk();
        return chunk?.GetBlockId(blockPos) ?? BlockId.Air;
    }

    public LightRgbi GetBlockLight(Vector3I blockPos)
    {
        var chunk = GetChunk(blockPos);
        blockPos = blockPos.BlockPosWorldToChunk();
        return chunk?.GetBlockLight(blockPos) ?? new();
    }
    
    public BlockData.Data GetBlockAllData(Vector3I blockPos)
    {
        var chunk = GetChunk(blockPos);
        blockPos = blockPos.BlockPosWorldToChunk();
        return chunk?.GetBlockAllData(blockPos) ?? new();
    }

    public void SetBlockId(Vector3I blockPos, BlockId id)
    {
        var chunk = GetChunk(blockPos);
        blockPos = blockPos.BlockPosWorldToChunk();
        if (chunk is not null)
            chunk.SetBlockId(blockPos, id);
        else
            UnloadedBlockIds[blockPos] = id;
    }

    public void SetBlockLight(Vector3I blockPos, LightRgbi light)
    {
        var chunk = GetChunk(blockPos);
        blockPos = blockPos.BlockPosWorldToChunk();
        chunk?.SetBlockLight(blockPos, light);
    }

    // public BlockData GetBlockData(Vector3I position)
    // {
    //     var offset = position.WorldBlockPosToChunkOffset();
    //     if (GetChunk(offset, out var chunk))
    //     {
    //         var pos = position.BlockPosWorldToChunk();
    //         return chunk.GetBlockData(pos);
    //     }
    //     // TODO: cache unloaded chunks' blocks
    //     if (UnloadedData.TryGetValue(position, out var data))
    //         return data;
    //     return new();
    // }
    //
    // public void SetBlockData(Vector3I position, BlockData data)
    // {
    //     var offset = position.WorldBlockPosToChunkOffset();
    //     if (GetChunk(offset, out var chunk))
    //     {
    //         var pos = position.BlockPosWorldToChunk();
    //         chunk.SetBlockData(pos, data);
    //     }
    //     else
    //     {
    //         // TODO: cache unloaded chunks' blocks
    //         UnloadedData[position] = data;
    //     }
    // }

    public void LoadEmptyChunks(GL gl)
    {
        var offsets = new List<Vector3I>();
        for (var x = 0; x < ChunksSize; x++)
        {
            for (var y = 0; y < ChunksSize; y++)
            {
                for (var z = 0; z < ChunksSize; z++)
                {
                    var offset = Vector3I.Add(ChunksOrigin, new(x, y, z));
                    offsets.Add(offset);
                }
            }
        }
        var comparer = new ChunkDepthComparer(CenterOffset, DepthOrder.Nearer);
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

    public void SetCenter(GL gl, Vector3I center)
    {
        var newOffset = center.BlockPosToChunkOffset();
        var newOrigin = Vector3I.Subtract(
            newOffset,
            // new(ChunksSize / 2 + 1, ChunksSize / 2 + 1, ChunksSize / 2 + 1)
            new(ChunksSize / 2, ChunksSize / 2, ChunksSize / 2)
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
        //TODO: may remove
        LoadEmptyChunks(gl);
    }

    public void Render(GL gl)
    {
        foreach (var offset in SortChunksByOffset(DepthOrder.Nearer))
        {
            if (GetChunk(offset, out var chunk))
                chunk.Prepare(gl);
        }

        var renderer = State.Renderer;
        renderer.UseShader(gl, Renderer.ShaderType.Chunk);
        renderer.PushCamera();
        renderer.SetCamera(CameraType.Perspective);
        renderer.SetViewProject(gl);

        //TODO: shit here
        var shader = renderer.Shaders[Renderer.ShaderType.Chunk];
        shader.UniformTexture(gl, "tex", renderer.BlockAtlas.Atlas.Texture);
        shader.UniformVector4(gl, "fog_color", renderer.ClearColor);
        shader.UniformFloat(gl, "fog_near", ChunksSize / 2f * 32 - 12);
        shader.UniformFloat(gl, "fog_far", ChunksSize / 2f * 32 - 4);

        foreach (var chunk in Chunks.Values)
            chunk.Render(gl, ChunkMesh.Part.Solid);
        foreach (var offset in SortChunksByOffset(DepthOrder.Farther))
        {
            if (GetChunk(offset, out var chunk))
                chunk.Render(gl, ChunkMesh.Part.Transparent);
        }

        Player.Render(gl);
        renderer.PopCamera();
    }

    public void Update(GL gl)
    {
        Loading.Reset();
        Meshing.Reset();
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
        foreach (var offset in Chunks.Keys)
            offsets.Add(offset);
        var comparer = new ChunkDepthComparer(CenterOffset, order);
        offsets.Sort(comparer);
        return offsets.ToArray();
    }
}
