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
    public const int ChunkSizeX = 32;
    public const int ChunkSizeY = 32;
    public const int ChunkSizeZ = 32;
    const int ChunkVolume = ChunkSizeX * ChunkSizeY * ChunkSizeZ;
    public static Vector2I ChunkSizeXz { get; } = new(ChunkSizeX, ChunkSizeZ);
    public static Vector3I ChunkSize { get; } = new(ChunkSizeX, ChunkSizeY, ChunkSizeZ);
    public World World { get; set; }
    public Vector3I Offset { get; private set; }
    Vector3I ChunckPosition { get; set; }
    Memory<BlockDataService?> Data { get; } = new BlockDataService?[ChunkVolume];
    ChunkMesh Mesh { get; }
    int NoneAirCount { get; set; }
    bool Empty => NoneAirCount is 0;
    public bool Generating { get; set; } = true;

    public Chunk(GL gl, World world, Vector3I offset)
    {
        World = world;
        Offset = offset;
        ChunckPosition = Vector3I.Multiply(offset, ChunkSize);
        Mesh = new(gl, this);
    }

    public void Delete(GL gl)
    {
        // Data = [];
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

    private BlockChunkPosition CreatePosition(Vector3I position)
    {
        return new(position, ChunckPosition);
    }

    public BlockChunkPosition CreatePosition(int x, int y, int z)
    {
        return CreatePosition(new Vector3I(x, y, z));
    }

    public BlockChunkPosition CreatePosition(BlockPosition position)
    {
        return CreatePosition(position.IntoChunk());
    }

    public Matrix4x4 CreateModel()
    {
        return Matrix4x4.CreateTranslation(ChunckPosition.ToNumerics());
    }

    public Vector3I[] GetBorderingChunkOffsets(Vector3I position)
    {
        var offsets = new List<Vector3I>(6);
        if (position.X is 0)
        {
            var offset = Vector3I.Add(Offset, -Vector3I.UnitX);
            offsets.Add(offset);
        }
        if (position.Y is 0)
        {
            var offset = Vector3I.Add(Offset, -Vector3I.UnitY);
            offsets.Add(offset);
        }
        if (position.Z is 0)
        {
            var offset = Vector3I.Add(Offset, -Vector3I.UnitZ);
            offsets.Add(offset);
        }
        if (position.X == ChunkSizeX - 1)
        {
            var offset = Vector3I.Add(Offset, Vector3I.UnitX);
            offsets.Add(offset);
        }
        if (position.Y == ChunkSizeY - 1)
        {
            var offset = Vector3I.Add(Offset, Vector3I.UnitY);
            offsets.Add(offset);
        }
        if (position.Z == ChunkSizeZ - 1)
        {
            var offset = Vector3I.Add(Offset, Vector3I.UnitZ);
            offsets.Add(offset);
        }
        return offsets.ToArray();
    }

    private List<Chunk> GetBorderingChunks(Vector3I position)
    {
        var offsets = GetBorderingChunkOffsets(position);
        var chunks = new List<Chunk>(offsets.Length);
        foreach (var offset in offsets)
        {
            var chunk = World.GetChunk(offset);
            if (chunk is not null)
                chunks.Add(chunk);
        }
        return chunks;
    }

    private void OnModify(Vector3I position, BlockData prev, BlockData changed)
    {
        if (!Generating)
            Mesh.Dirty = true;
        else
            Mesh.Dirty = false;

        if (prev.BlockId != changed.BlockId)
        {
            var wPos = CreatePosition(position).IntoWorld();
            if (prev.BlockId.Block().CanEmitLight)
                Light.RemoveAllLight(World, wPos);
            var block = changed.BlockId.Block();
            var torchLight = block.GetTorchLight();
            if (block.CanEmitLight)
                Light.AddTorchLight(World, wPos, torchLight);
            if (!Generating)
            {
                if (block.Transparent)
                {
                    World.RecaculateHeightmap(wPos);
                    Light.UpdateAllLight(World, wPos);
                }
                else
                {
                    World.UpdateHeightmap(wPos);
                    Light.RemoveAllLight(World, wPos);
                }
            }
            NoneAirCount += changed.BlockId is BlockId.Air ? -1 : 1;
        }

        // if (prev.BlockId != changed.BlockId || prev.AllLight != changed.AllLight)
        // {
        //     var neighbors = GetBorderingChunks(position);
        //     foreach (var chunk in neighbors)
        //         chunk.Mesh.Dirty = true;
        // }
    }

    private void RefreshNeighbors() { }

    public int GetHighest(BlockChunkPosition position)
    {
        var heightMap = World.GetHeightmap(Offset);
        if (heightMap is null)
            throw new ArgumentNullException();
        return heightMap.GetHeight(position);
    }

    public int GetHighest(BlockPosition position)
    {
        return World.GetHighest(position);
    }

    public Heightmap GetHeightmap()
    {
        return World.GetHeightmap(Offset) ?? throw new ArgumentNullException();
    }

    public void PrepareRender(GL gl)
    {
        if (!Empty)
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
        var withinDistance = Vector3I.Distance(Offset, player.ChunkOffset) < 4;
        var blockchanged = Offset == player.ChunkOffset && player.BlockPositionChanged;
        var chunkChanged = player.ChunkOffsetChanged && withinDistance;
        Mesh.DepthSort = blockchanged || chunkChanged;
        Mesh.SetPersist(withinDistance);
    }

    public void Tick() { }

    // MUST be run once a chunk has completed generating
    public void AfterGenerate()
    {
        RecaculateHeightmap();
        Light.ApplyAllLight(this);
        Mesh.Dirty = true;
    }

    private void RecaculateHeightmap()
    {
        var heightmap = GetHeightmap();
        for (var x = 0; x < ChunkSizeX; x++)
        {
            for (var z = 0; z < ChunkSizeZ; z++)
            {
                var h = heightmap.GetHeight(x, z);
                if (h > ChunckPosition.Y + ChunkSizeY - 1)
                    continue;
                for (var y = ChunkSizeY - 1; y >= 0; y--)
                {
                    var id = GetBlockId(new Vector3I(x, y, z));
                    if (!id.Block().Transparent)
                    {
                        var pos = CreatePosition(x, y, z).IntoWorld();
                        heightmap.SetHeight(pos);
                    }
                }
            }
        }
    }
}
