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

public sealed class ChunkMesh
{
    public enum Part : byte
    {
        Solid,
        Transparent,
    }

    UnsafeList<BlockVertex> _vertices = [];
    UnsafeList<Face> _transparentFaces = [];
    UnsafeList<uint> _transparentIndices = [];
    UnsafeList<uint> _solidIndices = [];
    uint _vertexCount = 0;
    Chunk Chunk { get; set; }
    bool HasTransparent { get; set; }
    bool HasSolid { get; set; }

    bool Finalize { get; set; } = true;
    public bool Dirty { get; set; } = true;

    public bool DepthSort { get; set; } = true;
    public bool Destroy { get; set; } = true;
    public bool Persist { get; set; } = true;
    VaoHandler Vao { get; }
    VboHandler Vbo { get; }
    IboHandler IboTransparent { get; }
    IboHandler IboSolid { get; }

    public ChunkMesh(GL gl, Chunk chunk)
    {
        Chunk = chunk;
        Vao = VaoHandler.Create(gl);
        Vbo = VboHandler.Create(gl, false);
        IboTransparent = IboHandler.Create(gl, false);
        IboSolid = IboHandler.Create(gl, false);
    }

    public unsafe void Delete(GL gl)
    {
        _vertices.Release();
        _transparentIndices.Release();
        _transparentFaces.Release();
        _solidIndices.Release();
        Vao.Delete(gl);
        Vbo.Delete(gl);
        IboTransparent.Delete(gl);
        IboSolid.Delete(gl);
    }

    public void MeshPrepare()
    {
        _vertices.Release();
        _transparentIndices.Release();
        _transparentFaces.Release();
        _solidIndices.Release();
        _vertexCount = 0;
    }

    // MUST be called immediately after meshing (before rendering)
    private void FinalizeData(GL gl)
    {
        if (_vertices.Count is 0)
            // throw new ArgumentNullException(nameof(Vertices));
            return;
        Vbo.Buffer<BlockVertex>(gl, _vertices.AsSpan());
        // _vertices.Release();
    }

    // MUST be called immediately after meshing AND sorting (before rendering)
    private void FinalizeIndices(GL gl)
    {
        IboTransparent.Buffer(gl, _transparentIndices.AsSpan());
        IboSolid.Buffer(gl, _solidIndices.AsSpan());
        // _solidIndices.Release();
        if (Persist)
            return;
        // _transparentIndices.Release();
        // _transparentFaces.Release();
    }

    private unsafe void SortTransparentFaces()
    {
        if (_transparentFaces.Count is 0)
            return;
        var center = Chunk.World.Player.Camera.Position;
        var comparer = new FaceDepthComparer(center, DepthOrder.Farther);
        _transparentFaces.AsSpan().Sort(comparer);
        
        var len = _transparentIndices.Count;
        var old = new UnsafeList<uint>(len);
        Utils.Memcpy(_transparentIndices.Data, old.Data, len * sizeof(uint));
        for (var i = 0; i < _transparentFaces.Count; i++)
        {
            var face = &_transparentFaces.Data[i];
            if (face->IndicesBase != i * 6)
            {
                Utils.Memcpy(
                    &old.Data[face->IndicesBase],
                    &_transparentIndices.Data[i * 6],
                    6 * sizeof(uint)
                );
            }
            face->IndicesBase = i * 6;
        }
        // old.Release();
    }

    private void Mesh(GL gl)
    {
        MeshPrepare();
        for (var x = 0; x < Chunk.ChunkSizeX; x++)
        {
            for (var z = 0; z < Chunk.ChunkSizeZ; z++)
            {
                for (var y = 0; y < Chunk.ChunkSizeY; y++)
                {
                    var data = Chunk.GetBlockData(x, y, z);
                    if (data.BlockId is BlockId.Air)
                        continue;
                    var block = data.BlockId.Block();
                    switch (block.MeshType)
                    {
                        case BlockMeshType.Sprite:
                        case BlockMeshType.Torch:
                        {
                            MeshSprite(x, y, z, block, data.AllLight);
                            break;
                        }
                        case BlockMeshType.Default:
                        case BlockMeshType.Liquid:
                            MeshDefault(x, y, z, block);
                            break;
                    }
                }
            }
        }

        SortTransparentFaces();
        FinalizeData(gl);
        FinalizeIndices(gl);

        // GC.Collect();
    }

    private void MeshSprite(int x, int y, int z, Block block, AllLight allLight)
    {
        GlobalState.Renderer.BlockAtlas.Atlas.GetUv(
            block.TextureLocation[Direction.North],
            out var uvMin,
            out var uvMax
        );
        BlockMesh.MeshSprite(
            ref _vertices,
            ref _transparentIndices,
            ref _transparentFaces,
            ref _vertexCount,
            new(x, y, z),
            allLight,
            uvMin,
            uvMax
        );
    }

    private void MeshDefault(int x, int y, int z, Block block)
    {
        foreach (var direction in Direction.AllDirections)
        {
            var nPos = Chunk.CreatePosition(x, y, z).ToNeighbor(direction);
            var nData = Chunk.World.GetBlockData(nPos);
            var nBlock = nData.BlockId.Block();
            if (
                (nBlock.Transparent && !block.Transparent)
                || (block.Transparent && block.Id != nBlock.Id)
            )
            {
                GlobalState.Renderer.BlockAtlas.Atlas.GetUv(
                    block.TextureLocation[direction],
                    out var uvMin,
                    out var uvMax
                );
                var allLight = nData.AllLight;
                if (block.Transparent)
                    BlockMesh.MeshTransparentFace(
                        ref _vertices,
                        ref _transparentIndices,
                        ref _transparentFaces,
                        ref _vertexCount,
                        new(x, y, z),
                        allLight,
                        uvMin,
                        uvMax,
                        direction
                    );
                else
                    BlockMesh.MeshSolidFace(
                        ref _vertices,
                        ref _solidIndices,
                        ref _vertexCount,
                        new(x, y, z),
                        allLight,
                        uvMin,
                        uvMax,
                        direction
                    );
            }
        }
    }

    public void PrepareRender(GL gl)
    {
        if (!Chunk.World.Meshing.UnderThreshold())
            return;
        if (Dirty)
        {
            Mesh(gl);
            Dirty = false;
            DepthSort = false;
            Chunk.World.Meshing.AddOne();
        }
        else if (DepthSort)
        {
            if (Persist && _transparentFaces.Count is not 0)
            {
                SortTransparentFaces();
                FinalizeIndices(gl);
            }
            else
                Mesh(gl);
            DepthSort = false;
            Chunk.World.Meshing.AddOne();
        }
    }

    public void RederTransparent(GL gl)
    {
        // if (HasTransparent)
            Render(gl, IboTransparent);
    }

    public void RenderSolid(GL gl)
    {
        // if (HasSolid)
            Render(gl, IboSolid);
    }

    private void Render(GL gl, IboHandler ibo)
    {
        // TODO: shit here
        var shader = GlobalState.Renderer.Shaders[Renderer.ShaderType.Chunk];
        var model = Chunk.CreateModel();
        shader.UniformMatrix4(gl, "m", model);

        Vao.Link(gl, Vbo, 0, 3, VertexAttribPointerType.Float, 0);
        Vao.Link(gl, Vbo, 1, 2, VertexAttribPointerType.Float, sizeof(float) * 3);
        Vao.Link(gl, Vbo, 2, 1, VertexAttribIType.UnsignedInt, sizeof(float) * 5);

        ibo.DrawElements(gl, GlobalState.Renderer.Wireframe);
    }

    public void SetPersist(bool persist)
    {
        if (Persist == persist)
            return;
        Persist = persist;
        if (!persist)
        {
            // _transparentIndices.Release();
            // _transparentFaces.Release();
        }
    }
}
