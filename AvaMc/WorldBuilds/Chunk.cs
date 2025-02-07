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

public sealed class Chunk
{
    public World World { get; set; }
    ChunkOffset Offset { get; set; }
    Vector3I ChunckPosition { get; set; }
    Dictionary<Vector3I, BlockData> Data { get; set; } = [];

    // bool Dirty { get; set; } = true;
    // bool DepthSort { get; set; } = true;

    // int BlockCount { get; set; }
    //
    // // if true, this chunk is generating
    // bool Generating { get; set; }
    // ChunkMesh Mesh { get; set; }
    ChunkMesh Mesh { get; }
    int NoneAirCount { get; set; }
    bool Empty => NoneAirCount is 0;

    public Chunk(GL gl, World world, Vector3I offset)
    {
        World = world;
        Offset = new(offset);
        ChunckPosition = Offset.ToChunkPosition();
        Mesh = new(gl, this);
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
        return new(new(x, y, z), ChunckPosition);
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

    private BlockData GetBlockData(Vector3I position)
    {
        if (Data.TryGetValue(position, out var data))
            return data;
        Data[position] = new();
        return Data[position];
    }

    public BlockId GetBlockId(BlockChunkPosition position)
    {
        return GetBlockId(position.ToInternal());
    }

    private BlockId GetBlockId(Vector3I position)
    {
        var data = GetBlockData(position);
        return data.Id;
    }

    public LightRgbi GetBlockLight(BlockChunkPosition position)
    {
        return GetBlockLight(position.ToInternal());
    }

    private LightRgbi GetBlockLight(Vector3I position)
    {
        var data = GetBlockData(position);
        return data.Light;
    }

    public BlockData.Data GetBlockAllData(BlockChunkPosition position)
    {
        return GetBlockAllData(position.ToInternal());
    }

    private BlockData.Data GetBlockAllData(Vector3I position)
    {
        var data = GetBlockData(position);
        return data.GetData();
    }

    // public BlockData.Data GetBlockAllData

    private void SetBlockId(Vector3I position, BlockId id)
    {
        var data = GetBlockData(position);
        if (data.Id == id)
            return;
        Mesh.Dirty = true;
        data.Id = id;
        var neighbors = GetBorderingChunks(position);
        foreach (var chunk in neighbors)
            chunk.Mesh.Dirty = true;
        var count = NoneAirCount + (data.Id is BlockId.Air ? -1 : 1);
        NoneAirCount = Math.Max(0, count);
    }

    public void SetBlockId(BlockChunkPosition position, BlockId id)
    {
        SetBlockId(position.ToInternal(), id);
    }

    public void SetBlockId(int x, int y, int z, BlockId id)
    {
        SetBlockId(new Vector3I(x, y, z), id);
    }

    public bool SetBlockId(BlockWorldPosition position, BlockId id)
    {
        var offset = position.ToChunkOffset();
        if (offset != Offset.ToInernal())
            return false;
        SetBlockId(position.ToChunk(), id);
        return true;
    }

    public void SetBlockLight(BlockChunkPosition position, LightRgbi light)
    {
        SetBlockLight(position.ToInternal(), light);
    }

    private void SetBlockLight(Vector3I position, LightRgbi light)
    {
        var data = GetBlockData(position);
        Mesh.Dirty = true;
        data.Light = light;
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
