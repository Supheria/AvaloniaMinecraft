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
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public sealed class ChunkMesh
{
    public enum Part : byte
    {
        Solid,
        Transparent,
    }

    List<BlockVertex> _vertices = [];
    List<Face> _transparentFaces = [];
    List<Face> _solidFaces = [];
    uint _vertexCount = 0;
    Chunk Chunk { get; set; }
    bool HasTransparent { get; set; }
    bool HasSolid { get; set; }

    bool Finalize { get; set; } = true;
    public bool Dirty { get; set; } = true;

    public bool DepthSort { get; set; } = true;
    public bool Destroy { get; set; } = true;
    public bool Persist { get; set; } = true;

    // List<Face> Faces { get; } = [];
    // int IndexCount { get; set; }

    // // if true, this mesh needs to be finalized (uploaded)
    // public bool Finalize { get; set; } = true;
    //
    // // if true, this mesh will be rebuilt next time it is rendered
    // public bool Dirty { get; set; } = true;
    //
    // // if true, this mesh will be depth sorted next time it is rendered
    // public bool DepthSort { get; set; } = true;
    //
    // // if true, this mesh will be destroyed when its data is next accessible
    // public bool Destroy { get; set; } = true;
    //
    // // if true, index and face buffers are kept in memory after building
    // public bool Persist { get; set; } = true;
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

    public void Delete(GL gl)
    {
        Vao.Delete(gl);
        Vbo.Delete(gl);
        IboTransparent.Delete(gl);
        IboSolid.Delete(gl);
    }

    public void MeshPrepare()
    {
        _vertices = [];
        _transparentFaces = [];
        _solidFaces = [];
        _vertexCount = 0;
    }

    // MUST be called immediately after meshing (before rendering)
    private void FinalizeData(GL gl)
    {
        if (_vertices.Count is 0)
            // throw new ArgumentNullException(nameof(Vertices));
            return;
        Vbo.Buffer(gl, _vertices);
        _vertices = [];
    }

    // MUST be called immediately after meshing AND sorting (before rendering)
    private void FinalizeIndices(GL gl)
    {
        HasTransparent = _transparentFaces.Count is not 0;
        if (HasTransparent)
        {
            var indices = new List<uint>();
            _transparentFaces.ForEach(f => indices.AddRange(f.Indices));
            IboTransparent.Buffer(gl, indices);
        }
        HasSolid = _solidFaces.Count is not 0;
        if (HasSolid)
        {
            var indices = new List<uint>();
            _solidFaces.ForEach(f => indices.AddRange(f.Indices));
            IboSolid.Buffer(gl, indices);
        }
        if (Persist)
            return;
        _transparentFaces = [];
        _solidFaces = [];
    }

    private void SortTransparentFaces()
    {
        if (_transparentFaces.Count is 0)
            return;
        // TODO: too complex
        var center = Chunk.World.Player.Camera.Position;
        foreach (var face in _transparentFaces)
            face.SetDistance(center);
        var comparer = new FaceDepthComparer(DepthOrder.Farther);
        _transparentFaces.Sort(comparer);
    }

    public void Mesh(GL gl)
    {
        MeshPrepare();
        for (var x = 0; x < Chunk.ChunkSizeX; x++)
        {
            for (var z = 0; z < Chunk.ChunkSizeZ; z++)
            {
                for (var y = 0; y < Chunk.ChunkSizeY; y++)
                {
                    var position = Chunk.CreatePosition(x, y, z);
                    var data = Chunk.GetBlockData(position);
                    if (data.BlockId is BlockId.Air)
                        continue;
                    var block = data.BlockId.Block();
                    switch (block.MeshType)
                    {
                        case BlockMeshType.Sprite:
                        case BlockMeshType.Torch:
                        {
                            MeshSprite(position, block, data.AllLight);
                            break;
                        }
                        case BlockMeshType.Default:
                        case BlockMeshType.Liquid:
                            MeshDefault(position, block);
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

    private void MeshSprite(BlockChunkPosition position, Block block, AllLight allLight)
    {
        GlobalState.Renderer.BlockAtlas.Atlas.GetUv(
            block.GetTextureLocation(Direction.North),
            out var uvMin,
            out var uvMax
        );
        BlockMesh.MeshSprite(
            ref _vertices,
            ref _transparentFaces,
            ref _vertexCount,
            position.ToNumerics(),
            allLight,
            uvMin,
            uvMax
        );
    }

    private void MeshDefault(BlockChunkPosition position, Block block)
    {
        foreach (var direction in Direction.AllDirections)
        {
            var nPos = position.ToNeighbor(direction);
            var nData = Chunk.World.GetBlockData(nPos);
            // continue;
            var nBlock = nData.BlockId.Block();
            if (
                (nBlock.Transparent && !block.Transparent)
                || (block.Transparent && block.Id != nBlock.Id)
            )
            {
                GlobalState.Renderer.BlockAtlas.Atlas.GetUv(
                    block.GetTextureLocation(direction),
                    out var uvMin,
                    out var uvMax
                );
                var allLight = nData.AllLight;
                if (block.Transparent)
                    BlockMesh.MeshFace(
                        ref _vertices,
                        ref _transparentFaces,
                        ref _vertexCount,
                        position.ToNumerics(),
                        allLight,
                        uvMin,
                        uvMax,
                        direction
                    );
                else
                    BlockMesh.MeshFace(
                        ref _vertices,
                        ref _solidFaces,
                        ref _vertexCount,
                        position.ToNumerics(),
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
        if (HasTransparent)
            Render(gl, IboTransparent);
    }

    public void RenderSolid(GL gl)
    {
        if (HasSolid)
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
            _transparentFaces = [];
            _solidFaces = [];
        }
    }
}
