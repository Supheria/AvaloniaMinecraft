using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Comparers;
using AvaMc.Coordinates;
using AvaMc.Extensions;
using AvaMc.Gfx;
using AvaMc.Util;
using Hexa.NET.Utilities;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public unsafe struct ChunkMesh
{
    public enum Part : byte
    {
        Solid,
        Transparent,
    }

    const int VerticesVolume = Chunk.ChunkVolume / 2 * 6 * 6;
    const int IndicesVolume = VerticesVolume;
    const int FacesVolume = Chunk.ChunkVolume / 2 * 6;

    public UnsafeList<BlockVertex> Vertices;

    public UnsafeList<uint> Indices;
    public UnsafeList<Face> Faces;
    public uint VertexCount = 0;

    bool HasTransparent { get; set; }
    bool HasSolid { get; set; }

    bool Finalize { get; set; } = true;
    public bool Dirty { get; set; } = true;

    public bool DepthSort { get; set; } = true;
    public bool Destroy { get; set; } = true;
    public bool Persist { get; set; } = true;
    readonly VaoHandler _vao;
    VboHandler* PVbo { get; }
    IboHandler _ibo;
    int SolidOffset { get; set; }
    uint SolidCount { get; set; }
    int TransparentOffset { get; set; }
    uint TransparentCount { get; set; }

    public ChunkMesh(GL gl)
    {
        Vertices = new(VerticesVolume);
        Indices = new(IndicesVolume);
        Faces = new(FacesVolume);

        _vao = VaoHandler.Create(gl);
        PVbo = VboHandler.CreatePointer(gl, false);
        _ibo = IboHandler.Create(gl, false);
    }

    public void Delete(GL gl)
    {
        Vertices.Release();
        Indices.Release();
        Faces.Release();

        _vao.Delete(gl);
        VboHandler.Release(gl, PVbo);
        _ibo.Delete(gl);
    }

    private void MeshPrepare()
    {
        Vertices.Release();
        Indices.Release();
        Faces.Release();
        Vertices = new(VerticesVolume);
        Indices = new(IndicesVolume);
        Faces = new(FacesVolume);
        VertexCount = 0;
    }

    private void FinalizeData(GL gl)
    {
        if (Vertices.Count is 0)
            // throw new ArgumentNullException(nameof(Vertices));
            return;
        PVbo->Buffer<BlockVertex>(gl, Vertices.AsSpan());
        Vertices.Release();
    }

    private void FinalizeIndices(GL gl)
    {
        if (Indices.Count is 0)
            return;
        _ibo.Buffer(gl, Indices.AsSpan());
        if (Persist)
            return;
        Indices.Release();
        Faces.Release();
    }

    private byte* NewBitmap(int size)
    {
        var len = size / 8 + 1;
        var p = Utils.AllocT<byte>(len);
        Utils.ZeroMemoryT(p, len);
        return p;
    }

    private static void SetBitmap(byte* bitmap, int n)
    {
        bitmap[n / 8] |= (byte)(1 << (n % 8));
    }

    private bool BitmapSetted(byte* bitmap, int n)
    {
        return (bitmap[n / 8] & (1 << (n % 8))) != 0;
    }

    private void SortTransparentFaces(bool full)
    {
        if (Indices.Count is 0)
            return;
        var center = Chunk.World.Player.Camera.Position;
        var comparer = new FaceDepthComparer(center, DepthOrder.Farther);
        Faces.AsSpan().Sort(comparer);

        if (!full)
        {
            if (Faces.Count is 0)
                return;
            var oldIndices = Utils.AllocT<uint>(Indices.Count);
            {
                Utils.MemcpyT(Indices.Data, oldIndices, Indices.Count);
                for (var i = 0; i < Faces.Count; i++)
                {
                    var pFace = Faces.GetPointer(i);
                    if (pFace->IndicesBase != i * 6)
                        Utils.MemcpyT(&oldIndices[pFace->IndicesBase], &Indices.Data[i * 6], 6);
                    pFace->IndicesBase = i * 6;
                }
            }
            Utils.Free(oldIndices);
            return;
        }

        var moved = NewBitmap(Indices.Count / 6);
        {
            var oldIndices = Utils.AllocT<uint>(Indices.Count);
            {
                Utils.MemcpyT(Indices.Data, oldIndices, Indices.Count);
                for (var i = 0; i < Faces.Count; i++)
                {
                    var pFace = Faces.GetPointer(i);
                    if (pFace->IndicesBase != i * 6)
                        Utils.MemcpyT(&oldIndices[pFace->IndicesBase], &Indices.Data[i * 6], 6);

                    SetBitmap(moved, pFace->IndicesBase / 6);
                    pFace->IndicesBase = i * 6;
                }

                TransparentOffset = 0;
                SolidOffset = Faces.Count * 6;
                TransparentCount = (uint)SolidOffset;
                var solidCount = 0;
                for (var i = 0; i < Indices.Count / 6; i++)
                {
                    if (BitmapSetted(moved, i))
                        continue;
                    Utils.MemcpyT(&oldIndices[i * 6], &Indices.Data[SolidOffset + solidCount], 6);
                    solidCount += 6;
                }
                SolidCount = (uint)solidCount;
            }
            Utils.Free(oldIndices);
        }
        Utils.Free(moved);
    }

    private void Mesh(GL gl, ref Chunk chunk)
    {
        MeshPrepare();
        for (var x = 0; x < Chunk.ChunkSizeX; x++)
        {
            for (var z = 0; z < Chunk.ChunkSizeZ; z++)
            {
                for (var y = 0; y < Chunk.ChunkSizeY; y++)
                {
                    var data = chunk.GetBlockData(x, y, z);
                    if (data.BlockId is BlockId.Air)
                        continue;
                    var block = data.BlockId.Block();
                    switch (block->MeshType)
                    {
                        case BlockMeshType.Sprite:
                        case BlockMeshType.Torch:
                        {
                            MeshSprite(x, y, z, block, data.AllLight);
                            break;
                        }
                        case BlockMeshType.Default:
                        case BlockMeshType.Liquid:
                            MeshDefault(ref chunk, x, y, z, block);
                            break;
                    }
                }
            }
        }
        if (chunk.Offset == new Vector3I(11, 2, 7)) { }

        SortTransparentFaces(true);
        FinalizeData(gl);
        FinalizeIndices(gl);
    }

    private void MeshSprite(int x, int y, int z, Block* block, AllLight allLight)
    {
        GlobalState.Renderer.BlockAtlas.Atlas.GetUv(
            block->TextureLocation[Direction.North],
            out var uvMin,
            out var uvMax
        );
        BlockMesh.MeshSprite(ref this, new(x, y, z), allLight, uvMin, uvMax);
    }

    private void MeshDefault(ref Chunk chunk, int x, int y, int z, Block* block)
    {
        foreach (var direction in Direction.AllDirections)
        {
            var nPos = chunk.CreatePosition(x, y, z).ToNeighbor(direction);
            var nData = Chunk.World.GetBlockData(nPos);
            var nBlock = nData.BlockId.Block();
            if (
                (nBlock->Transparent && !block->Transparent)
                || (block->Transparent && block->Id != nBlock->Id)
            )
            {
                GlobalState.Renderer.BlockAtlas.Atlas.GetUv(
                    block->TextureLocation[direction],
                    out var uvMin,
                    out var uvMax
                );
                var allLight = nData.AllLight;
                BlockMesh.MeshFace(
                    ref this,
                    new(x, y, z),
                    allLight,
                    uvMin,
                    uvMax,
                    block->Transparent,
                    direction
                );
            }
        }
    }

    public void PrepareRender(GL gl, ref Chunk chunk)
    {
        if (!Chunk.World.Meshing.UnderThreshold())
            return;
        if (Dirty)
        {
            Mesh(gl, ref chunk);
            Dirty = false;
            DepthSort = false;
            Chunk.World.Meshing.AddOne();
        }
        else if (DepthSort)
        {
            if (Persist && Indices.Count != 0 && Faces.Count != 0)
            {
                SortTransparentFaces(false);
                FinalizeIndices(gl);
            }
            else
                Mesh(gl, ref chunk);
            DepthSort = false;
            Chunk.World.Meshing.AddOne();
        }
    }

    public void RederTransparent(GL gl, ref Chunk chunk)
    {
        if (chunk.Offset == new Vector3I(11, 2, 7)) { }
        if (TransparentCount != 0)
            Render(gl, ref chunk, TransparentOffset, TransparentCount);
    }

    public void RenderSolid(GL gl, ref Chunk chunk)
    {
        if (chunk.Offset == new Vector3I(11, 2, 7)) { }
        if (SolidCount != 0)
            Render(gl, ref chunk, SolidOffset, SolidCount);
    }

    private void Render(GL gl, ref Chunk chunk, int offset, uint count)
    {
        // TODO: shit here
        var shader = GlobalState.Renderer.Shaders[Renderer.ShaderType.Chunk];
        var model = chunk.CreateModel();
        shader.UniformMatrix4(gl, "m", model);

        _vao.Link(gl, PVbo, 0, 3, VertexAttribPointerType.Float, 0);
        _vao.Link(gl, PVbo, 1, 2, VertexAttribPointerType.Float, sizeof(float) * 3);
        _vao.Link(gl, PVbo, 2, 1, VertexAttribIType.UnsignedInt, sizeof(float) * 5);

        _ibo.DrawElements(gl, GlobalState.Renderer.Wireframe, offset, count);
        // _ibo.DrawElements(gl, GlobalState.Renderer.Wireframe);
    }

    public void SetPersist(bool persist)
    {
        if (Persist == persist)
            return;
        Persist = persist;
        if (persist)
            return;
        Indices.Release();
        Faces.Release();
    }
}
