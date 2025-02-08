using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Extensions;
using AvaMc.Gfx;
using AvaMc.Util;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public sealed partial class Chunk
{
    public World World { get; set; }
    ChunkOffset Offset { get; set; }
    Vector3I ChunckPosition { get; set; }
    Dictionary<Vector3I, BlockData> Data { get; set; } = [];
    ChunkMesh Mesh { get; }
    int NoneAirCount { get; set; }
    bool Empty => NoneAirCount is 0;
    bool Generating { get; set; } = true;

    public Chunk(GL gl, World world, Vector3I offset)
    {
        World = world;
        Offset = new(offset);
        ChunckPosition = Offset.ToChunkPosition();
        Mesh = new(gl, this);
    }

    public void Generate(WorldGenerator generator)
    {
        Generating = true;
        generator.Generate(this);
        Generating = false;
    }

    public void Delete(GL gl)
    {
        Data = [];
        Mesh.Delete(gl);
    }

    public long GetOffsetHashCode()
    {
        var h = 0L;
        var v = Offset.ToNumerics();
        for (var i = 0; i < 3; i++)
        {
            h ^= (int)v[i] + 0x9e3779b9 + (h << 6) + (h >> 2);
        }
        return h;
    }

    public BlockChunkPosition CreatePosition(int x, int y, int z)
    {
        var position = new Vector3I(x, y, z);
        return new(position, ChunckPosition);
    }

    public BlockChunkPosition CreatePosition(BlockWorldPosition position)
    {
        return new(position.ToChunk(), ChunckPosition);
    }

    public Matrix4x4 CreateModel()
    {
        return Matrix4x4.CreateTranslation(ChunckPosition.ToNumerics());
    }

    public ChunkOffset[] GetBorderingChunkOffsets(Vector3I position)
    {
        var offsets = new List<ChunkOffset>(6);
        if (position.X is 0)
        {
            offsets.Add(Offset.ToNeighbor(Direction.West));
        }
        if (position.Y is 0)
        {
            offsets.Add(Offset.ToNeighbor(Direction.Down));
        }
        if (position.Z is 0)
        {
            offsets.Add(Offset.ToNeighbor(Direction.North));
        }
        if (position.X == ChunkData.ChunkSizeX - 1)
        {
            offsets.Add(Offset.ToNeighbor(Direction.East));
        }
        if (position.Y == ChunkData.ChunkSizeY - 1)
        {
            offsets.Add(Offset.ToNeighbor(Direction.Up));
        }
        if (position.Z == ChunkData.ChunkSizeZ - 1)
        {
            offsets.Add(Offset.ToNeighbor(Direction.South));
        }
        return offsets.ToArray();
    }

    private List<Chunk> GetBorderingChunks(Vector3I position)
    {
        var offsets = GetBorderingChunkOffsets(position);
        var chunks = new List<Chunk>(offsets.Length);
        foreach (var offset in offsets)
        {
            if (World.GetChunk(offset, out var chunk))
                chunks.Add(chunk);
        }
        return chunks;
    }

    private void OnModify(Vector3I position, BlockData.Data prev, BlockData.Data changed)
    {
        Mesh.Dirty = true;

        if (prev.Id != changed.Id)
        {
            var pos = new BlockChunkPosition(position, ChunckPosition);
            if (changed.Id is BlockId.Air)
            {
                NoneAirCount--;
                World.RecaculateHeightmap(pos.ToWorld());
                if (!Generating)
                    Light.UpdateAllLight(World, pos.ToWorld());
            }
            else
            {
                NoneAirCount++;
                World.UpdateHeightmap(pos.ToWorld());
                if (!Generating)
                    Light.RemoveAllLight(World, pos.ToWorld());
            }
            NoneAirCount = Math.Max(0, NoneAirCount);
        }

        if (prev.Id != changed.Id || prev.AllLight != changed.AllLight)
        {
            var neighbors = GetBorderingChunks(position);
            foreach (var chunk in neighbors)
                chunk.Mesh.Dirty = true;
        }
    }

    public List<BlockChunkPosition> GetBlockPositions()
    {
        var result = new List<BlockChunkPosition>(Data.Count);
        foreach (var pos in Data.Keys)
        {
            result.Add(new(pos, ChunckPosition));
        }
        return result;
    }

    public int GetHighest(BlockChunkPosition position)
    {
        var heightMap = World.GetHeightmap(Offset);
        return heightMap.Get(position) - ChunckPosition.Y;
    }

    public int GetHighest(BlockWorldPosition position)
    {
        return World.GetHighest(position);
    }

    public void PrepareRender(GL gl)
    {
        if (Empty)
            return;
        Mesh.PrepareRender(gl);
    }

    public void RenderTransparent(GL gl)
    {
        if (!Empty)
            Mesh.RederTransparent(gl);
    }

    public void RenderSolid(GL gl)
    {
        if (!Empty)
            Mesh.RenderSolid(gl);
    }

    public void Update()
    {
        var player = World.Player;
        var withinDistance = Offset.Distance(player.ChunkOffset) < 4;
        var blockchanged = Offset == player.ChunkOffset && player.BlockPositionChanged;
        var chunkChanged = player.ChunkOffsetChanged && withinDistance;
        Mesh.DepthSort = blockchanged || chunkChanged;
        Mesh.SetPersist(withinDistance);
    }

    public void Tick() { }

    // /// <summary>
    // /// MUST be run once a chunk has completed generating
    // /// </summary>
    // public void AfterGenerate()
    // {
    //     World.HeightMapRecaculate(this);
    // }
}
