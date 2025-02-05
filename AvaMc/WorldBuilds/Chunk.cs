using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Extensions;
using AvaMc.Util;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public sealed class Chunk
{
    public World World { get; set; }
    public Vector3I Offset { get; set; }
    public Vector3I Position { get; set; }
    Dictionary<Vector3I, BlockData> Data { get; set; } = [];
    // bool Dirty { get; set; } = true;
    // bool DepthSort { get; set; } = true;
    bool Empty { get; set; } = true;

    // int BlockCount { get; set; }
    //
    // // if true, this chunk is generating
    // bool Generating { get; set; }
    // ChunkMesh Mesh { get; set; }
    ChunkMesh Mesh { get; }

    public Chunk(GL gl, World world, Vector3I offset)
    {
        World = world;
        Offset = offset;
        Position = Vector3I.Multiply(offset, ChunkData.ChunkSize);
        Mesh = new(gl, this);
    }

    public void Delete(GL gl)
    {
        Data = [];
        Mesh.Delete(gl);
    }

    public static bool InBounds(Vector3I pos)
    {
        return pos.X >= 0
            && pos.Y >= 0
            && pos.Z >= 0
            && pos.X < ChunkData.ChunkSizeX
            && pos.Y < ChunkData.ChunkSizeY
            && pos.Z < ChunkData.ChunkSizeZ;
    }

    public static bool OnBounds(Vector3I pos)
    {
        return pos.X is 0
            || pos.Y is 0
            || pos.Z is 0
            || pos.X is ChunkData.ChunkSizeX - 1
            || pos.Y is ChunkData.ChunkSizeY - 1
            || pos.Z is ChunkData.ChunkSizeZ - 1;
    }

    public List<Chunk> GetBorderingChunks(Vector3I pos)
    {
        var chunks = new List<Chunk>();
        if (pos.X is 0)
        {
            if (World.GetChunk(Vector3I.Add(Offset, new(-1, 0, 0)), out var chunk))
                chunks.Add(chunk);
        }
        // if (pos.Y is 0)
        // {
        //     if (World.GetChunk(Vector3I.Add(Offset, new(0, -1, 0)), out var chunk))
        //         chunks.Add(chunk);
        // }
        if (pos.Z is 0)
        {
            if (World.GetChunk(Vector3I.Add(Offset, new(0, 0, -1)), out var chunk))
                chunks.Add(chunk);
        }
        if (pos.X == ChunkData.ChunkSizeX - 1)
        {
            if (World.GetChunk(Vector3I.Add(Offset, new(1, 0, 0)), out var chunk))
                chunks.Add(chunk);
        }
        // if (pos.Y == ChunkData.ChunkSizeY - 1)
        // {
        //     if (World.GetChunk(Vector3I.Add(Offset, new(0, 1, 0)), out var chunk))
        //         chunks.Add(chunk);
        // }
        if (pos.Z == ChunkData.ChunkSizeZ - 1)
        {
            if (World.GetChunk(Vector3I.Add(Offset, new(0, 0, 1)), out var chunk))
                chunks.Add(chunk);
        }
        return chunks;
    }

    // public static Vector3 BlockPositionToChunkOffset(Vector3 pos)
    // {
    //     return Vector3.Divide(pos, ChunkData.ChunkSize);
    // }

    public void SetBlockData(Vector3I position, BlockData data)
    {
        if (!InBounds(position))
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position,
                "block position out chunk"
            );
        if (Data.TryGetValue(position, out var prevData) && data.BlockId != prevData.BlockId)
        {
            Data[position] = data;
            Mesh.Dirty = true;
        }
        else
        {
            Data[position] = data;
            Mesh.Dirty = true;
        }
        Empty = Data.Count is 0;
        if (OnBounds(position))
        {
            var neighbors = GetBorderingChunks(position);
            foreach (var chunk in neighbors)
                chunk.Mesh.Dirty = true;
        }
    }

    public BlockData GetBlockData(Vector3I position)
    {
        if (Data.TryGetValue(position, out var data))
            return data;
        Data[position] = new BlockData { BlockId = BlockId.Air };
        return Data[position];
    }

    public Vector3I[] GetBlockPositions()
    {
        return Data.Keys.ToArray();
    }

    public BlockData GetBlockDataInOtherChunk(Vector3I position)
    {
        var pos = position + Position;
        return World.GetBlockData(pos);
    }

    public void SetBlockDataInOtherChunk(Vector3I position, BlockData data)
    {
        var pos = position + Position;
        World.SetBlockData(pos, data);
    }

    // public static Vector2 BlockPositionToChunkOffset(Vector2 pos)
    // {
    //     return Vector2.Divide(pos, ChunkData.ChunkSizeF.Xz());
    // }
    
    public void Prepare(GL gl)
    {
        if (Empty)
            return;
        Mesh.PrepareRender(gl);
    }
    
    public void Render(GL gl, ChunkMesh.Part part)
    {
        if (Empty)
            return;
        Mesh.Render(gl, part);
    }

    public void Update()
    {
        var player = World.Player;
        var withinDistance = Vector3I.Distance(Offset, player.ChunkOffset) < 3;
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
