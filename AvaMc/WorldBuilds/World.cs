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
    // const int ChunksMagnitude = 10;
    const int ChunksMagnitude = 4;
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
        // LoadEmptyChunks(gl);
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
