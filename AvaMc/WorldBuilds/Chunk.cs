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
    int ChunkX { get; }
    int ChunkY { get; }
    int ChunkZ { get; }
    BlockData[] Data { get; } = new BlockData[ChunkVolume];
    ChunkMesh Mesh { get; }
    int NoneAirCount { get; set; }
    bool Empty => NoneAirCount is 0;
    public bool Generating { get; set; } = true;

    public Chunk(GL gl, World world, Vector3I offset)
    {
        World = world;
        Offset = offset;
        var pos = Vector3I.Multiply(offset, ChunkSize);
        ChunkX = pos.X;
        ChunkY = pos.Y;
        ChunkZ = pos.Z;
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

    private static int PositionToIndex(int x, int y, int z)
    {
        var index = x * ChunkSizeX * ChunkSizeZ + z * ChunkSizeZ + y;
        return index;
    }

    public BlockChunkPosition CreatePosition(int x, int y, int z)
    {
        return new(x, y, z, ChunkX, ChunkY, ChunkZ);
    }

    public Matrix4x4 CreateModel()
    {
        return Matrix4x4.CreateTranslation(new(ChunkX, ChunkY, ChunkZ));
    }

    public Vector3I[] GetBorderingChunkOffsets(int x, int y, int z)
    {
        var offsets = new List<Vector3I>(6);
        if (ChunkX is 0)
        {
            var offset = Vector3I.Add(Offset, -Vector3I.UnitX);
            offsets.Add(offset);
        }
        if (ChunkY is 0)
        {
            var offset = Vector3I.Add(Offset, -Vector3I.UnitY);
            offsets.Add(offset);
        }
        if (ChunkZ is 0)
        {
            var offset = Vector3I.Add(Offset, -Vector3I.UnitZ);
            offsets.Add(offset);
        }
        if (ChunkX == ChunkSizeX - 1)
        {
            var offset = Vector3I.Add(Offset, Vector3I.UnitX);
            offsets.Add(offset);
        }
        if (ChunkY == ChunkSizeY - 1)
        {
            var offset = Vector3I.Add(Offset, Vector3I.UnitY);
            offsets.Add(offset);
        }
        if (ChunkZ == ChunkSizeZ - 1)
        {
            var offset = Vector3I.Add(Offset, Vector3I.UnitZ);
            offsets.Add(offset);
        }
        return offsets.ToArray();
    }

    private List<Chunk> GetBorderingChunks(int x, int y, int z)
    {
        var offsets = GetBorderingChunkOffsets(x, y, z);
        var chunks = new List<Chunk>(offsets.Length);
        foreach (var offset in offsets)
        {
            var chunk = World.GetChunk(offset);
            if (chunk is not null)
                chunks.Add(chunk);
        }
        return chunks;
    }

    private void OnModify(int x, int y, int z, BlockData prev, BlockData changed)
    {
        Mesh.Dirty = true;

        if (prev.BlockId != changed.BlockId)
        {
            var wPos = CreatePosition(x, y, z).IntoWorld();
            if (prev.BlockId.Block().CanEmitLight)
                Light.RemoveAllLight(World, wPos);
            var block = changed.BlockId.Block();
            if (block.CanEmitLight)
                Light.AddTorchLight(World, wPos, block.TorchLight);
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

        if (prev.BlockId != changed.BlockId || prev.AllLight != changed.AllLight)
        {
            var neighbors = GetBorderingChunks(x, y, z);
            foreach (var chunk in neighbors)
                chunk.Mesh.Dirty = true;
        }
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
        // Mesh.Dirty = true;
        // foreach (var chunk in GetAllBorderingChunks())
        //     chunk.Mesh.Dirty = true;
    }

    public List<Chunk> GetAllBorderingChunks()
    {
        var offsets = new[]
        {
            Vector3I.Add(Offset, -Vector3I.UnitX),
            Vector3I.Add(Offset, -Vector3I.UnitY),
            Vector3I.Add(Offset, -Vector3I.UnitZ),
            Vector3I.Add(Offset, Vector3I.UnitX),
            Vector3I.Add(Offset, Vector3I.UnitY),
            Vector3I.Add(Offset, Vector3I.UnitZ),
        };
        var chunks = new List<Chunk>(offsets.Length);
        foreach (var offset in offsets)
        {
            var chunk = World.GetChunk(offset);
            if (chunk is not null)
                chunks.Add(chunk);
        }
        return chunks;
    }

    private void RecaculateHeightmap()
    {
        var heightmap = GetHeightmap();
        for (var x = 0; x < ChunkSizeX; x++)
        {
            for (var z = 0; z < ChunkSizeZ; z++)
            {
                var h = heightmap.GetHeight(x, z);
                if (h > ChunkY + ChunkSizeY - 1)
                    continue;
                for (var y = ChunkSizeY - 1; y >= 0; y--)
                {
                    var id = GetBlockId(x, y, z);
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
