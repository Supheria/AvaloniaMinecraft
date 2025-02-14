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
using Hexa.NET.Utilities;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

// TODO
public sealed unsafe partial class World
{
    // const int ChunksMagnitude = 10;
    int ChunksMagnitude { get; set; } = 4;
    int ChunksVolume => ChunksMagnitude * ChunksMagnitude * ChunksMagnitude;
    int HeightmapsVolume => ChunksMagnitude * ChunksMagnitude;
    Sky Sky { get; }
    public Player Player { get; set; }
    IntPtr* _chunkPointers;
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
        // Seed = new Random().Next();
        Seed = 10;
        Generator = new(Seed);

        _chunkPointers = Utils.AllocT<IntPtr>(ChunksVolume);
        Utils.ZeroMemoryT(_chunkPointers, ChunksVolume);

        Heightmaps = new Heightmap[HeightmapsVolume];
        // SetCenter(gl, BlockPosition.Zero);
    }

    public unsafe void Delete(GL gl)
    {
        Sky.Delete(gl);
        Player.Delete(gl);
        for (var i = 0; i < ChunksVolume; i++)
        {
            var pChunk = (Chunk*)_chunkPointers[i];
            if (pChunk != null)
                pChunk->Delete(gl);
        }
        Utils.Free(_chunkPointers);
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

    public unsafe void SetCenter(GL gl, BlockPosition position)
    {
        var newOffset = position.ToChunkOffset();
        var magnitude = new Vector3I(ChunksMagnitude / 2, ChunksMagnitude / 2, ChunksMagnitude / 2);
        var newOrigin = Vector3I.Subtract(newOffset, magnitude);
        if (ChunksOrigin == newOrigin)
            return;
        CenterChunkOffset = newOffset;
        ChunksOrigin = newOrigin;

        var old = Utils.AllocT<IntPtr>(ChunksVolume);
        {
            Utils.MemcpyT(_chunkPointers, old, ChunksVolume);
            Utils.ZeroMemoryT(_chunkPointers, ChunksVolume);
            for (var i = 0; i < ChunksVolume; i++)
            {
                var pChunk = (Chunk*)old[i];
                if (pChunk == null)
                    continue;
                if (ChunkInBounds(pChunk->Offset))
                {
                    var index = ChunkOffsetToIndex(pChunk->Offset);
                    _chunkPointers[index] = (IntPtr)pChunk;
                }
                else
                    pChunk->Delete(gl);
            }
        }
        Utils.Free(old);

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

        var offsets = new UnsafeList<Vector3I>();
        {
            SortChunksByOffset(DepthOrder.Nearer, ref offsets);
            // TODO: use pointer of offset
            for (var i = 0; i < offsets.Count; i++)
            {
                if (GetChunk(offsets[i], out var ptr))
                    ptr->PrepareRender(gl);
            }
        }
        offsets.Release();

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

        for (var i = 0; i < ChunksVolume; i++)
        {
            var pChunk = (Chunk*)_chunkPointers[i];
            if (pChunk != null)
                pChunk->RenderSolid(gl);
        }

        offsets = new UnsafeList<Vector3I>();
        {
            SortChunksByOffset(DepthOrder.Nearer, ref offsets);
            for (var i = 0; i < offsets.Count; i++)
            {
                if (GetChunk(offsets[i], out var ptr))
                    ptr->RenderTransparent(gl);
            }
        }
        offsets.Release();

        Player.Render(gl);
        renderer.PopCamera();
    }

    public void Update(GL gl)
    {
        Loading.Reset();
        Meshing.Reset();
        LoadEmptyChunks(gl);
        for (var i = 0; i < ChunksVolume; i++)
        {
            var pChunk = (Chunk*)_chunkPointers[i];
            if (pChunk != null)
                pChunk->Update();
        }
        Player.Update();
    }

    public void Tick()
    {
        Ticks++;
        for (var i = 0; i < ChunksVolume; i++)
        {
            var pChunk = (Chunk*)_chunkPointers[i];
            if (pChunk != null)
                pChunk->Tick();
        }
        Player.Tick();
    }

    private void SortChunksByOffset(DepthOrder order, ref UnsafeList<Vector3I> offsets)
    {
        for (var i = 0; i < ChunksVolume; i++)
        {
            var pChunk = (Chunk*)_chunkPointers[i];
            if (pChunk is not null)
                offsets.Add(pChunk->Offset);
        }
        var comparer = new ChunkDepthComparer(CenterChunkOffset, order);
        offsets.AsSpan().Sort(comparer);
    }
}
