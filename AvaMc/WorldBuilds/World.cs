using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AvaMc.Blocks;
using AvaMc.Comparers;
using AvaMc.Coordinates;
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

    const int ChunksMagnitude = 8;
    public Player Player { get; set; }
    Dictionary<Vector3I, Chunk> Chunks { get; set; } = [];
    Vector3I ChunksOrigin { get; set; }
    Vector3I CenterChunkOffset { get; set; }
    public Threshold Loading { get; } = new(1);
    public Threshold Meshing { get; } = new(8);

    // TODO: use dictionary
    public Dictionary<BlockWorldPosition, BlockId> UnloadedBlockIds { get; } = [];
    WorldGenerator Generator { get; } = new(2);

    public World(GL gl)
    {
        Player = new(this);
        SetCenter(gl, BlockWorldPosition.Zero);
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
        var modified = Vector3I.Subtract(offset, ChunksOrigin);
        return modified.X >= 0
            && modified.Y >= 0
            && modified.Z >= 0
            && modified.X < ChunksMagnitude
            && modified.Y < ChunksMagnitude
            && modified.Z < ChunksMagnitude;
    }

    public bool GetChunk(ChunkOffset offset, [NotNullWhen(true)] out Chunk? chunk)
    {
        return GetChunk(offset.ToInernal(), out chunk);
    }

    private bool GetChunk(Vector3I offset, [NotNullWhen(true)] out Chunk? chunk)
    {
        chunk = null;
        if (!ChunkInBounds(offset))
            return false;
        return Chunks.TryGetValue(offset, out chunk);
    }

    // TODO: cache unloaded chunks' blocks
    private Chunk? GetChunk(BlockWorldPosition position)
    {
        var offset = position.ToChunkOffset();
        if (GetChunk(offset, out var chunk))
            return chunk;
        return null;
    }

    public BlockId GetBlockId(BlockWorldPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return BlockId.Air;
        var cPos = chunk.CreatePosition(position);
        return chunk.GetBlockId(cPos);
    }

    public BlockId GetBlockId(BlockChunkPosition position)
    {
        var wPos = position.ToWorld();
        return GetBlockId(wPos);
    }

    public LightRgbi GetBlockLight(BlockWorldPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return new();
        var cPos = chunk.CreatePosition(position);
        return chunk.GetBlockLight(cPos);
    }

    public BlockData.Data GetBlockAllData(BlockWorldPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return new();
        var cPos = chunk.CreatePosition(position);
        return chunk.GetBlockAllData(cPos);
    }

    public BlockData.Data GetBlockAllData(BlockChunkPosition position)
    {
        var wPos = position.ToWorld();
        return GetBlockAllData(wPos);
    }

    public void SetBlockId(BlockWorldPosition position, BlockId id)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            UnloadedBlockIds[position] = id;
        else
        {
            var cPos = chunk.CreatePosition(position);
            chunk.SetBlockId(cPos, id);
        }
    }

    public void SetBlockId(BlockChunkPosition position, BlockId id)
    {
        var wPos = position.ToWorld();
        SetBlockId(wPos, id);
    }

    public void SetBlockLight(BlockWorldPosition position, LightRgbi light)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return;
        var cPos = chunk.CreatePosition(position);
        chunk.SetBlockLight(cPos, light);
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
        var chunk = new Chunk(gl, this, offset);
        Generator.Generate(chunk);
        Chunks[offset] = chunk;
    }

    public void SetCenter(GL gl, BlockWorldPosition position)
    {
        var newOffset = position.ToChunkOffset();
        var magnitude = new Vector3I(ChunksMagnitude / 2, ChunksMagnitude / 2, ChunksMagnitude / 2);
        var newOrigin = Vector3I.Subtract(newOffset, magnitude);
        if (ChunksOrigin == newOrigin)
            return;
        CenterChunkOffset = newOffset;
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
                chunk.PrepareRender(gl);
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
        shader.UniformFloat(gl, "fog_near", ChunksMagnitude / 2f * 32 - 12);
        shader.UniformFloat(gl, "fog_far", ChunksMagnitude / 2f * 32 - 4);

        foreach (var chunk in Chunks.Values)
            chunk.RenderSolid(gl);
        foreach (var offset in SortChunksByOffset(DepthOrder.Farther))
        {
            if (GetChunk(offset, out var chunk))
                chunk.RenderTransparent(gl);
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

    private List<Vector3I> SortChunksByOffset(DepthOrder order)
    {
        var offsets = Chunks.Keys.ToList();
        var comparer = new ChunkDepthComparer(CenterChunkOffset, order);
        offsets.Sort(comparer);
        return offsets;
    }
}
