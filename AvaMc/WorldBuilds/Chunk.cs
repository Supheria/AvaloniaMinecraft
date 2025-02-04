using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Extensions;
using AvaMc.Util;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public sealed class Chunk
{
    public enum MeshPass
    {
        Full,
        Transparency,
    }

    public World World { get; set; }
    public Vector3I Offset { get; set; }
    public Vector3I Position { get; set; }
    Dictionary<Vector3I, BlockData> Data { get; set; } = [];
    bool Dirty { get; set; }
    bool DepthSort { get; set; }

    // int BlockCount { get; set; }
    //
    // // if true, this chunk contains no blocks
    // bool Empty { get; set; } = true;
    //
    // // if true, this chunk is generating
    // bool Generating { get; set; }
    // ChunkMesh Mesh { get; set; }
    ChunkMesh BaseMesh { get; set; }
    ChunkMesh TransparentMesh { get; set; }

    public Chunk(GL gl, World world, Vector3I offset)
    {
        World = world;
        Offset = offset;
        Position = Vector3I.Multiply(offset, ChunkData.ChunkSize);
        BaseMesh = new(gl, this);
        TransparentMesh = new(gl, this);
    }

    public void Delete(GL gl)
    {
        BaseMesh.Delete(gl);
        TransparentMesh.Delete(gl);
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

    public List<Chunk> GetBorderingChunks(Vector3I position)
    {
        var chunks = new List<Chunk>();
        if (position.X is 0)
        {
            if (World.GetChunk(Vector3I.Add(Offset, new(-1, 0, 0)), out var chunk))
                chunks.Add(chunk);
        }
        // if (position.Y is 0)
        // {
        //     if (World.GetChunk(Vector3.Add(Offset, new(0, -1, 0)), out var chunk))
        //         chunks.Add(chunk);
        // }
        if (position.Z is 0)
        {
            if (World.GetChunk(Vector3I.Add(Offset, new(0, 0, -1)), out var chunk))
                chunks.Add(chunk);
        }
        if (position.X == ChunkData.ChunkSizeX - 1)
        {
            if (World.GetChunk(Vector3I.Add(Offset, new(1, 0, 0)), out var chunk))
                chunks.Add(chunk);
        }
        // if ((int)Position.Y == ChunkData.ChunkSizeY - 1)
        // {
        //     if (World.GetChunk(Vector3.Add(Offset, new(0, 1, 0)), out var chunk))
        //         chunks.Add(chunk);
        // }
        if (Position.Z == ChunkData.ChunkSizeZ - 1)
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

    public void SetData(Vector3I position, BlockData data)
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

    public BlockData GetBlockData(Vector3I position)
    {
        if (Data.TryGetValue(position, out var data))
            return data;
        Data[position] = new BlockData { BlockId = BlockId.Air };
        return Data[position];
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

    public void Mesh(GL gl, MeshPass pass)
    {
        if (pass is MeshPass.Full)
            BaseMesh.Prepare();
        TransparentMesh.Prepare();

        for (var x = 0; x < ChunkData.ChunkSizeX; x++)
        {
            for (var y = 0; y < ChunkData.ChunkSizeY; y++)
            {
                for (var z = 0; z < ChunkData.ChunkSizeZ; z++)
                {
                    MeshBlock(new(x, y, z), pass);
                }
            }
        }

        if (pass is MeshPass.Full)
            BaseMesh.Finalize(gl, false);
        TransparentMesh.Finalize(gl, true);
    }

    private void MeshBlock(Vector3I pos, MeshPass pass)
    {
        var data = GetBlockData(pos);
        var block = Block.Blocks[data.BlockId];
        // TODO: when air may has bug
        if (block.Id is BlockId.Air)
            return;
        var transparent = block.Transparent;

        if (block.Sprite)
        {
            var uv = Block.Blocks[data.BlockId].GetTextureLocation(Direction.North);
            var uvOffset = State.BlockAtlas.Atlas.Offset(uv);
            TransparentMesh.EmitSprite(
                pos.ToNumerics(),
                uvOffset,
                State.BlockAtlas.Atlas.SpriteUnit
            );
        }
        else
            MeshNotSprite(pos, pass, transparent, block);
    }

    private void MeshNotSprite(Vector3I pos, MeshPass pass, bool transparent, Block block)
    {
        foreach (var direction in Direction.AllDirections)
        {
            var dv = direction.Vector3I;
            var neighbor = Vector3I.Add(pos, dv);

            Block neighborBlock;
            if (InBounds(neighbor))
            {
                var nData = GetBlockData(neighbor);
                neighborBlock = Block.Blocks[nData.BlockId];
            }
            else
            {
                var nData = GetBlockDataInOtherChunk(neighbor);
                neighborBlock = Block.Blocks[nData.BlockId];
            }
            var neighborTransparent = neighborBlock.Transparent;

            if (
                neighborTransparent && (pass is MeshPass.Full && !transparent)
                || (transparent && neighborBlock.Id != block.Id)
            )
            {
                var uv = block.GetTextureLocation(direction);
                var uvOffset = State.BlockAtlas.Atlas.Offset(uv);
                var shortenY =
                    block.Liquid && direction.Value is Direction.Type.Up && !neighborBlock.Liquid;
                var mesh = transparent ? TransparentMesh : BaseMesh;
                mesh.EmitFace(
                    pos.ToNumerics(),
                    direction,
                    uvOffset,
                    State.BlockAtlas.Atlas.SpriteUnit,
                    transparent,
                    shortenY
                );
            }
        }
    }

    public void Render(GL gl)
    {
        if ((Dirty || DepthSort) && World.Mesh.UnderThreshold())
        {
            // Debug.WriteLineIf(!Dirty && DepthSort, $"DepthSort{DateTime.Now}");
            Mesh(gl, Dirty ? MeshPass.Full : MeshPass.Transparency);
            Dirty = false;
            DepthSort = false;
            World.Mesh.AddOne();
        }
        BaseMesh.Render(gl);
    }

    public void RenderTransparent(GL gl)
    {
        TransparentMesh.Render(gl);
    }

    public void Update()
    {
        var player = World.Player;
        var blockchanged = Offset == player.ChunkOffset && player.BlockPositionChanged;
        var chunkChanged =
            player.ChunkOffsetChanged && Vector3I.Distance(Offset, player.ChunkOffset) < 2;
        DepthSort = blockchanged || chunkChanged;
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
