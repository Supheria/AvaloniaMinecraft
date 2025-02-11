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
    int ChunksMagnitude { get; set; } = 4;
    int ChunksVolume => ChunksMagnitude * ChunksMagnitude * ChunksMagnitude;
    int HeightmapsVolume => ChunksMagnitude * ChunksMagnitude;
    Sky Sky { get; }
    public Player Player { get; set; }
    Chunk?[] Chunks { get; set; }
    Heightmap?[] Heightmaps { get; set; }
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
        Chunks = new Chunk?[ChunksVolume];
        Heightmaps = new Heightmap[HeightmapsVolume];
        // SetCenter(gl, BlockPosition.Zero);
    }

    public void Delete(GL gl)
    {
        Sky.Delete(gl);
        Player.Delete(gl);
        foreach (var chunk in Chunks.AsSpan())
            chunk?.Delete(gl);
        Chunks = [];
    }

    private int ChunkOffsetToIndex(Vector3I offset)
    {
        var p = Vector3I.Subtract(offset, ChunksOrigin);
        return p.X * ChunksMagnitude * ChunksMagnitude + p.Z * ChunksMagnitude + p.Y;
    }

    private Vector3I ChunkIndexToOffset(int index)
    {
        var p = new Vector3I(
            index / (ChunksMagnitude * ChunksMagnitude),
            index % ChunksMagnitude,
            index / ChunksMagnitude % ChunksMagnitude
        );
        return Vector3I.Add(ChunksOrigin, p);
    }

    private int HeightmapOffsetToIndex(Vector2I offset)
    {
        var p = Vector2I.Subtract(offset, ChunksOrigin.Xz());
        return p.X * ChunksMagnitude + p.Y;
    }

    private Vector2I HeightmapIndexToOffset(int index)
    {
        var p = new Vector2I(index / ChunksMagnitude, index % ChunksMagnitude);
        return Vector2I.Add(ChunksOrigin.Xz(), p);
    }

    private bool ChunkInBounds(Vector3I offset)
    {
        var modified = Vector3I.Subtract(offset, ChunksOrigin);
        return modified.X >= 0
            && modified.Y >= 0
            && modified.Z >= 0
            && modified.X < ChunksMagnitude
            && modified.Y < ChunksMagnitude
            && modified.Z < ChunksMagnitude;
    }

    private bool HeightmapInBounds(Vector2I offset)
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

        var newChunks = new Chunk?[ChunksVolume];
        var cSpan = newChunks.AsSpan();
        var cSpanOld = Chunks.AsSpan();
        for (var i = 0; i < ChunksVolume; i++)
        {
            var chunk = cSpanOld[i];
            if (chunk is null)
                continue;
            if (ChunkInBounds(chunk.Offset))
            {
                var index = ChunkOffsetToIndex(chunk.Offset);
                cSpan[index] = chunk;
            }
            else
                chunk.Delete(gl);
        }
        Chunks = newChunks;

        var newHeightmaps = new Heightmap?[HeightmapsVolume];
        var hSpan = newHeightmaps.AsSpan();
        var hSpanOld = Heightmaps.AsSpan();
        for (var i = 0; i < HeightmapsVolume; i++)
        {
            var heightmap = hSpanOld[i];
            if (heightmap is null)
                continue;
            if (!HeightmapInBounds(heightmap.Offset))
                continue;
            var index = HeightmapOffsetToIndex(heightmap.Offset);
            hSpan[index] = heightmap;
        }
        Heightmaps = newHeightmaps;

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
            var chunk = GetChunk(offset);
            chunk?.PrepareRender(gl);
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

        foreach (var chunk in Chunks.AsSpan())
            chunk?.RenderSolid(gl);
        foreach (var offset in SortChunksByOffset(DepthOrder.Farther))
        {
            var chunk = GetChunk(offset);
            chunk?.RenderTransparent(gl);
        }

        Player.Render(gl);
        renderer.PopCamera();
    }

    public void Update(GL gl)
    {
        Loading.Reset();
        Meshing.Reset();
        LoadEmptyChunks(gl);
        foreach (var chunk in Chunks.AsSpan())
            chunk?.Update();
        Player.Update();
    }

    public void Tick()
    {
        Ticks++;
        foreach (var chunk in Chunks.AsSpan())
            chunk?.Tick();
        Player.Tick();
    }

    private Span<Vector3I> SortChunksByOffset(DepthOrder order)
    {
        var length = Chunks.Length;
        var offsets = new Vector3I[length].AsSpan();
        var cSpan = Chunks.AsSpan();
        for (var i = 0; i < length; i++)
        {
            var chunk = cSpan[i];
            if (chunk is not null)
                offsets[i] = chunk.Offset;
        }
        var comparer = new ChunkDepthComparer(CenterChunkOffset, order);
        offsets.Sort(comparer);
        return offsets;
    }
}
