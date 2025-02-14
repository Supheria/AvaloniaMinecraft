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
using Hexa.NET.Utilities;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public unsafe partial struct Chunk
{
    public const int ChunkSizeX = 32;
    public const int ChunkSizeY = 32;
    public const int ChunkSizeZ = 32;
    public const int ChunkVolume = ChunkSizeX * ChunkSizeY * ChunkSizeZ;
    public static Vector2I ChunkSizeXz { get; } = new(ChunkSizeX, ChunkSizeZ);
    public static Vector3I ChunkSize { get; } = new(ChunkSizeX, ChunkSizeY, ChunkSizeZ);

    public static World World => GlobalState.World;
    public static Chunk Default => new();
    public Vector3I Offset { get; private set; }
    int ChunkX { get; }
    int ChunkY { get; }
    int ChunkZ { get; }
    BlockData* _data;
    ChunkMesh _mesh;
    int NoneAirCount { get; set; }
    bool Empty => NoneAirCount is 0;
    public bool Generating { get; set; }

    // public bool Generated { get; set; }

    public unsafe Chunk(GL gl, Vector3I offset)
    {
        // World = world;
        Offset = offset;
        var pos = Vector3I.Multiply(offset, ChunkSize);
        ChunkX = pos.X;
        ChunkY = pos.Y;
        ChunkZ = pos.Z;
        
        _data = Utils.AllocT<BlockData>(ChunkVolume);
        Utils.ZeroMemoryT(_data, ChunkVolume);
        
        _mesh = new(gl);
    }

    public void Delete(GL gl)
    {
        // Data = [];
        Utils.Free(_data);
        _mesh.Delete(gl);
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

    private void GetBorderingChunkOffsets(int x, int y, int z, ref UnsafeList<Vector3I> offsets)
    {
        if (x is 0)
        {
            var offset = Vector3I.Add(Offset, -Vector3I.UnitX);
            offsets.Add(offset);
        }
        if (y is 0)
        {
            var offset = Vector3I.Add(Offset, -Vector3I.UnitY);
            offsets.Add(offset);
        }
        if (z is 0)
        {
            var offset = Vector3I.Add(Offset, -Vector3I.UnitZ);
            offsets.Add(offset);
        }
        if (x == ChunkSizeX - 1)
        {
            var offset = Vector3I.Add(Offset, Vector3I.UnitX);
            offsets.Add(offset);
        }
        if (y == ChunkSizeY - 1)
        {
            var offset = Vector3I.Add(Offset, Vector3I.UnitY);
            offsets.Add(offset);
        }
        if (z == ChunkSizeZ - 1)
        {
            var offset = Vector3I.Add(Offset, Vector3I.UnitZ);
            offsets.Add(offset);
        }
    }

    private void OnModify(int x, int y, int z, BlockData prev, BlockData changed)
    {
        _mesh.Dirty = true;

        if (prev.BlockId != changed.BlockId)
        {
            var wPos = CreatePosition(x, y, z).IntoWorld();
            if (prev.BlockId.Block()->CanEmitLight)
                Light.RemoveAllLight(World, wPos);
            var block = changed.BlockId.Block();
            if (block->CanEmitLight)
                Light.AddTorchLight(World, wPos, block->TorchLight);
            if (!Generating)
            {
                if (block->Transparent)
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
            var offsets = new UnsafeList<Vector3I>(6);
            {
                GetBorderingChunkOffsets(x, y, z, ref offsets);
                for (var i = 0; i < offsets.Count; i++)
                {
                    if (World.GetChunk(offsets[i], out var pChunk))
                        pChunk->_mesh.Dirty = true;
                }
            }
            offsets.Release();
        }
    }

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
            _mesh.PrepareRender(gl, ref this);
    }

    public void RenderTransparent(GL gl)
    {
        if (Offset == new Vector3I(11, 1, 7))
        {
            
        }
        if (!Empty)
            _mesh.RederTransparent(gl, ref this);
    }

    public void RenderSolid(GL gl)
    {
        if (Offset == new Vector3I(11, 1, 7))
        {
            
        }
        if (!Empty)
            _mesh.RenderSolid(gl, ref this);
    }

    public void Update()
    {
        var player = World.Player;
        var withinDistance = Vector3I.Distance(Offset, player.ChunkOffset) < 4;
        var blockchanged = Offset == player.ChunkOffset && player.BlockPositionChanged;
        var chunkChanged = player.ChunkOffsetChanged && withinDistance;
        _mesh.DepthSort = blockchanged || chunkChanged;
        _mesh.SetPersist(withinDistance);
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

    // public List<Chunk> GetAllBorderingChunks()
    // {
    //     var offsets = new[]
    //     {
    //         Vector3I.Add(Offset, -Vector3I.UnitX),
    //         Vector3I.Add(Offset, -Vector3I.UnitY),
    //         Vector3I.Add(Offset, -Vector3I.UnitZ),
    //         Vector3I.Add(Offset, Vector3I.UnitX),
    //         Vector3I.Add(Offset, Vector3I.UnitY),
    //         Vector3I.Add(Offset, Vector3I.UnitZ),
    //     };
    //     var chunks = new List<Chunk>(offsets.Length);
    //     foreach (var offset in offsets)
    //     {
    //         var chunk = World.GetChunk(offset);
    //         if (chunk is not null)
    //             chunks.Add(chunk);
    //     }
    //     return chunks;
    // }

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
                    if (!id.Block()->Transparent)
                    {
                        var pos = CreatePosition(x, y, z).IntoWorld();
                        heightmap.SetHeight(pos);
                    }
                }
            }
        }
    }
}
