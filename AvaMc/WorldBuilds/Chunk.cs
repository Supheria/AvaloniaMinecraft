using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvaMc.Blocks;
using AvaMc.Extensions;
using AvaMc.Util;
using Microsoft.Xna.Framework;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public class Chunk
{
    public const int ChunkSizeX = 16;
    public const int ChunkSizeY = 256;
    public const int ChunkSizeZ = 16;
    public static Vector3 ChunkSize { get; } = new(ChunkSizeX, ChunkSizeY, ChunkSizeZ);
    public World World { get; set; }
    public Vector3 Offset { get; set; }
    public Vector3 Position { get; set; }
    Dictionary<Vector3, BlockData> Data { get; set; } = [];
    bool Dirty { get; set; }

    // int BlockCount { get; set; }
    //
    // // if true, this chunk contains no blocks
    // bool Empty { get; set; } = true;
    //
    // // if true, this chunk is generating
    // bool Generating { get; set; }
    ChunkMesh Mesh { get; set; }

    public Chunk(GL gl, World world, Vector3 offset)
    {
        World = world;
        Offset = offset;
        Position = Vector3.Multiply(offset, ChunkSize);
        Mesh = new ChunkMesh(gl, this);
    }

    public void Delete(GL gl)
    {
        Mesh.Delete(gl);
    }

    public static bool InBounds(Vector3 pos)
    {
        return pos.X >= 0
            && pos.Y >= 0
            && pos.Z >= 0
            && pos.X < ChunkSizeX
            && pos.Y < ChunkSizeY
            && pos.Z < ChunkSizeZ;
    }

    public static bool OnBounds(Vector3 pos)
    {
        return pos.X is 0
            || pos.Y is 0
            || pos.Z is 0
            || pos.X is ChunkSizeX - 1
            || pos.Y is ChunkSizeY - 1
            || pos.Z is ChunkSizeZ - 1;
    }

    public List<Chunk> GetBorderingChunks(Vector3 position)
    {
        var chunks = new List<Chunk>();
        if (position.X is 0)
        {
            if (World.GetChunk(Vector3.Add(Offset, new(-1, 0, 0)), out var chunk))
                chunks.Add(chunk);
        }
        // if (position.Y is 0)
        // {
        //     if (World.GetChunk(Vector3.Add(Offset, new(0, -1, 0)), out var chunk))
        //         chunks.Add(chunk);
        // }
        if (position.Z is 0)
        {
            if (World.GetChunk(Vector3.Add(Offset, new(0, 0, -1)), out var chunk))
                chunks.Add(chunk);
        }
        if ((int)position.X == ChunkSizeX - 1)
        {
            if (World.GetChunk(Vector3.Add(Offset, new(1, 0, 0)), out var chunk))
                chunks.Add(chunk);
        }
        // if ((int)Position.Y == ChunkSizeY - 1)
        // {
        //     if (World.GetChunk(Vector3.Add(Offset, new(0, 1, 0)), out var chunk))
        //         chunks.Add(chunk);
        // }
        if ((int)Position.Z == ChunkSizeZ - 1)
        {
            if (World.GetChunk(Vector3.Add(Offset, new(0, 0, 1)), out var chunk))
                chunks.Add(chunk);
        }
        return chunks;
    }

    // public static Vector3 BlockPositionToChunkOffset(Vector3 pos)
    // {
    //     return Vector3.Divide(pos, ChunkSize);
    // }

    public void SetData(Vector3 position, BlockData data)
    {
        if (!InBounds(position))
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position,
                "block position out chunk"
            );
        Data[position] = data;
        Dirty = true;
        if (OnBounds(position))
        {
            var neighbors = GetBorderingChunks(position);
            foreach (var chunk in neighbors)
                chunk.Dirty = true;
        }
    }

    public bool GetData(Vector3 position, out BlockData data)
    {
        return Data.TryGetValue(position, out data);
    }

    public static Vector2 BlockPositionToChunkOffset(Vector2 pos)
    {
        return Vector2.Divide(pos, ChunkSize.Xz());
    }

    public void DoMesh(GL gl)
    {
        Mesh.Prepare();
        // foreach (var direction in Direction.AllDirections)
        // {
        //     Mesh.EmitFace(direction);
        // }
            
        for (var x = 0; x < ChunkSizeX; x++)
        {
            for (var y = 0; y < ChunkSizeY; y++)
            {
                for (var z = 0; z < ChunkSizeZ; z++)
                {
                    var pos = new Vector3(x, y, z);
                    var wPos = Vector3.Add(pos, Position);
                    if (!GetData(pos, out var data))
                        continue;
                    foreach (var direction in Direction.AllDirections)
                    {
                        var dv = direction.Vector3;
                        var neighbor = Vector3.Add(pos, dv);
                        var wNeighbor = Vector3.Add(wPos, dv);
                        var visible = true;
                        if (InBounds(neighbor))
                        {
                            if (GetData(neighbor, out var nData))
                            {
                                var block = Block.Blocks[nData.BlockId];
                                visible = block.Transparent;
                            }
                        }
                        else
                        {
                            var offset = BlockPositionToChunkOffset(wNeighbor);
                            // visible = World.ChunkInBounds(offset);
                            if (World.GetChunk(offset, out var chunk))
                            {
                                var p = WorldPosToChunkPos(wNeighbor);
                                if (chunk.GetData(p, out var nData))
                                {
                                    var block = Block.Blocks[nData.BlockId];
                                    visible = block.Transparent;
                                }
                            }
                        }
                        if (visible)
                        {
                            var uv = Block.Blocks[data.BlockId].GetTextureLocation(direction);
                            var uvOffset = State.Atlas.Offset(uv);
                            Mesh.EmitFace(pos, direction, uvOffset, State.Atlas.SpriteUnit);
                        }
                    }
                }
            }
        }
        Mesh.Finalize(gl);
    }
    
    public void Render(GL gl)
    {
        if (Dirty)
        {
            DoMesh(gl);
            Dirty = false;
        }
        Mesh.Render(gl);
    }

    public static Vector3 WorldPosToChunkPos(Vector3 pos)
    {
        var size = ChunkSize;
        return pos.Mod(size).Add(size).Mod(size);
    }

    public static Vector3 BlockPositionToChunkOffset(Vector3 pos)
    {
        return new((int)MathF.Floor(pos.X / ChunkSizeX), 0, (int)MathF.Floor(pos.Z / ChunkSizeZ));
    }

    // /// <summary>
    // /// MUST be run once a chunk has completed generating
    // /// </summary>
    // public void AfterGenerate()
    // {
    //     World.HeightMapRecaculate(this);
    // }
}
