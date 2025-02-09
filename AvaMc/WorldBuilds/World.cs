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
public sealed partial class World
{
    // const int ChunksSize = 28;

    const int ChunksMagnitude = 10;
    Sky Sky { get; }
    public Player Player { get; set; }
    Dictionary<Vector3I, Chunk> Chunks { get; set; } = [];
    Vector3I ChunksOrigin { get; set; }
    Vector3I CenterChunkOffset { get; set; }
    public Threshold Loading { get; } = new(1);
    public Threshold Meshing { get; } = new(8);

    // TODO: use dictionary
    public Dictionary<BlockPosition, BlockId> UnloadedBlockIds { get; } = [];
    WorldGenerator Generator { get; }
    public long Ticks { get; set; }
    public int Seed { get; set; }

    public World(GL gl)
    {
        Sky = new(gl, this);
        Player = new(this);
        Seed = new Random().Next();
        Generator = new(Seed);
        SetCenter(gl, BlockPosition.Zero);
    }

    public void Delete(GL gl)
    {
        Sky.Delete(gl);
        Player.Delete(gl);
        foreach (var chunk in Chunks.Values)
            chunk.Delete(gl);
        Chunks.Clear();
    }

    private bool InBounds(Vector3I offset)
    {
        var modified = Vector3I.Subtract(offset, ChunksOrigin);
        return modified.X >= 0
            && modified.Y >= 0
            && modified.Z >= 0
            && modified.X < ChunksMagnitude
            && modified.Y < ChunksMagnitude
            && modified.Z < ChunksMagnitude;
    }

    private bool InBounds(Vector2I offset)
    {
        var modified = Vector2I.Subtract(offset, ChunksOrigin.Xz());
        return modified.X >= 0
            && modified.Y >= 0
            && modified.X < ChunksMagnitude
            && modified.Y < ChunksMagnitude;
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
        var cPos = chunk.CreatePosition(position);
        return chunk.GetBlockId(cPos);
    }

    public LightIbgrs GetAllLight(BlockPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return new();
        var cPos = chunk.CreatePosition(position);
        return chunk.GetAllLight(cPos);
    }

    public BlockData.Data GetBlockData(BlockPosition position)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return new();
        var cPos = chunk.CreatePosition(position);
        return chunk.GetBlockData(cPos);
    }

    public void SetBlockId(BlockPosition position, BlockId id)
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

    public void SetAllLight(BlockPosition position, LightIbgrs light)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return;
        var cPos = chunk.CreatePosition(position);
        chunk.SetAllLight(cPos, light);
    }

    public void SetSunlight(BlockPosition position, int sunlight)
    {
        var chunk = GetChunk(position);
        if (chunk is null)
            return;
        var cPos = chunk.CreatePosition(position);
        chunk.SetSunlight(cPos, sunlight);
    }

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

    public void SetCenter(GL gl, BlockPosition position)
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
            if (InBounds(offset))
                continue;
            chunk.Delete(gl);
            Chunks.Remove(offset);
        }

        foreach (var offset in Heightmaps.Keys)
        {
            if (!InBounds(offset))
                Heightmaps.Remove(offset);
        }

        //TODO: may remove
        LoadEmptyChunks(gl);
    }

    public void Render(GL gl)
    {
        Sky.FogNear = ChunksMagnitude / 2f * 32 - 12;
        Sky.FogFar = ChunksMagnitude / 2f * 32 - 4;
        Sky.Render(gl);
        GlobalState.Renderer.ClearColor = Sky.ClearColor;

        foreach (var offset in SortChunksByOffset(DepthOrder.Nearer))
        {
            if (GetChunk(offset, out var chunk))
                chunk.PrepareRender(gl);
        }

        var renderer = GlobalState.Renderer;
        renderer.UseShader(gl, Renderer.ShaderType.Chunk);
        renderer.PushCamera();
        renderer.SetCamera(CameraType.Perspective);
        renderer.SetViewProject(gl);

        //TODO: shit here
        var shader = renderer.Shaders[Renderer.ShaderType.Chunk];
        shader.UniformTexture(gl, "tex", renderer.BlockAtlas.Atlas.Texture);
        shader.UniformVector4(gl, "sunlight_color", Sky.SunlgihtColor);

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
        Ticks++;
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
