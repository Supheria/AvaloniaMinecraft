using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Comparers;
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

    Chunk Chunk { get; set; }
    public List<ChunkVertex> Vertices { get; set; } = [];
    public List<Face> TransparentFaces { get; set; } = [];
    public List<Face> SolidFaces { get; set; } = [];

    // public List<uint> Indices { get; set; } = [];
    uint VertexCount { get; set; }
    bool Finalize { get; set; } = true;
    public bool Dirty { get; set; } = true;

    public bool DepthSort { get; set; } = true;
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

    // private static int DepthSortCompare(Vector3 center, Face face1, Face face2)
    // {
    //     var l1 = Vector3.Subtract(center, face1.Position).LengthSquared();
    //     var l2 = Vector3.Subtract(center, face2.Position).LengthSquared();
    //     return -Math.Sign(l1 - l2);
    // }

    public void MeshPrepare()
    {
        Vertices = [];
        TransparentFaces = [];
        SolidFaces = [];
        VertexCount = 0;
    }

    // MUST be called immediately after meshing (before rendering)
    public void FinalizeData(GL gl)
    {
        if (Vertices.Count is 0)
            // throw new ArgumentNullException(nameof(Vertices));
            return;
        Vbo.Buffer(gl, Vertices);
        Vertices = [];
    }

    // MUST be called immediately after meshing AND sorting (before rendering)
    public void FinalizeIndices(GL gl)
    {
        if (TransparentFaces.Count is 0 && SolidFaces.Count is 0)
            // throw new ArgumentException();
            return;
        var indices = new List<uint>();
        TransparentFaces.ForEach(f => indices.AddRange(f.Indices));
        IboTransparent.Buffer(gl, indices);
        indices = [];
        SolidFaces.ForEach(f => indices.AddRange(f.Indices));
        IboSolid.Buffer(gl, indices);
        if (Persist)
            return;
        TransparentFaces = [];
        SolidFaces = [];
    }

    public void EmitSprite(Vector3 position, Vector2 uvOffset, Vector2 uvUnit)
    {
        for (var i = 0; i < 8; i++)
        {
            var index = i * 3;
            var x = ChunkData.CubeVertices[index++] + position.X;
            var y = ChunkData.CubeVertices[index++] + position.Y;
            var z = ChunkData.CubeVertices[index] + position.Z;
            index = (i % 4) * 2;
            var u = ChunkData.CubeUvs[index++] * uvUnit.X + uvOffset.X;
            var v = ChunkData.CubeUvs[index] * uvUnit.Y + uvOffset.Y;

            // TODO: fix this
            var vertex = new ChunkVertex(new(x, y, z), new(u, v), new());
            Vertices.Add(vertex);
        }

        for (var i = 0; i < 2; i++)
        {
            var indices = new uint[6];
            for (var j = 0; j < 6; j++)
                indices[j] = ChunkData.SpriteIndices[i][j] + VertexCount;
            var face = new Face(indices, position);
            TransparentFaces.Add(face);
        }

        VertexCount += 8;
    }

    public void EmitFace(
        Vector3 position,
        Direction direction,
        Vector2 uvOffset,
        Vector2 uvUnit,
        LightRgbi light,
        bool transparent,
        bool shortenY
    )
    {
        for (var i = 0; i < 4; i++)
        {
            var index = ChunkData.CubeIndices[(direction * 6) + ChunkData.UniqueIndices[i]] * 3;
            var x = position.X + ChunkData.CubeVertices[index++];
            var yFactor = shortenY ? 0.9f : 1f;
            var y = position.Y + ChunkData.CubeVertices[index++] * yFactor;
            var z = position.Z + ChunkData.CubeVertices[index];
            index = i * 2;
            var u = uvOffset.X + (uvUnit.X * ChunkData.CubeUvs[index++]);
            var v = uvOffset.Y + (uvUnit.Y * ChunkData.CubeUvs[index]);
            // float color;
            // if (transparent)
            //     color = 1;
            // else
            // {
            //     switch (direction.Value)
            //     {
            //         case Direction.Type.Up:
            //             color = 1;
            //             break;
            //         case Direction.Type.North:
            //         case Direction.Type.South:
            //             color = 0.86f;
            //             break;
            //         case Direction.Type.East:
            //         case Direction.Type.West:
            //             color = 0.8f;
            //             break;
            //         case Direction.Type.Down:
            //             color = 0.6f;
            //             break;
            //         default:
            //             color = 0;
            //             break;
            //     }
            // }
            // var light = neighborData.Light;
            var vertex = new ChunkVertex(new(x, y, z), new(u, v), light);
            Vertices.Add(vertex);
        }

        var indices = new uint[6];
        for (var i = 0; i < 6; i++)
            indices[i] = VertexCount + ChunkData.FaceIndices[i];
        var center = ChunkData.FaceCenters[direction];
        var face = new Face(indices, Vector3.Add(center, position));
        if (transparent)
            TransparentFaces.Add(face);
        else
            SolidFaces.Add(face);

        VertexCount += 4;
    }

    private void SortTransparentFaces()
    {
        if (TransparentFaces.Count is 0)
            return;
        var center = Chunk.World.Player.Camera.Position;
        for (var i = 0; i < TransparentFaces.Count; i++)
            TransparentFaces[i].SetDistance(center);
        var comparer = new FaceDepthComparer(DepthOrder.Farther);
        TransparentFaces.Sort(comparer);
    }

    public void Mesh(GL gl)
    {
        MeshPrepare();

        foreach (var position in Chunk.GetBlockPositions())
            Mesh(position);

        SortTransparentFaces();
        FinalizeData(gl);
        FinalizeIndices(gl);

        GC.Collect();
    }

    private void Mesh(Vector3I pos)
    {
        var blockId = Chunk.GetBlockId(pos);
        // var block = Block.Blocks[data.BlockId];
        // TODO: when air may has bug
        if (blockId is BlockId.Air)
            return;

        var block = blockId.Block();
        if (block.Sprite)
        {
            // var uv = block.GetTextureLocation(Direction.North);
            // // shit here
            // var uvOffset = State.Renderer.BlockAtlas.Atlas.Offset(uv);
            // var spriteUnit = State.Renderer.BlockAtlas.Atlas.SpriteUnit;
            // EmitSprite(pos.ToNumerics(), uvOffset, spriteUnit);
        }
        else
            MeshNotSprite(pos, blockId);
        DepthSort = false;
        Chunk.World.Meshing.AddOne();
    }

    private void MeshNotSprite(Vector3I pos, BlockId blockId)
    {
        foreach (var direction in Direction.AllDirections)
        {
            var dv = direction.Vector3I;
            var neighborPos = Vector3I.Add(pos, dv);

            var neighborData = Chunk.InBounds(neighborPos)
                ? Chunk.GetBlockAllData(neighborPos)
                : Chunk.World.GetBlockAllData(neighborPos + Chunk.Position);

            var block = blockId.Block();
            var neighbor = neighborData.Id.Block();
            if (
                (neighbor.Transparent && !block.Transparent)
                || (block.Transparent && neighborData.Id != blockId)
            )
            {
                var uv = block.GetTextureLocation(direction);
                // TODO: shit here
                var uvOffset = State.Renderer.BlockAtlas.Atlas.Offset(uv);
                var spriteUnit = State.Renderer.BlockAtlas.Atlas.SpriteUnit;
                var shortenY = block.Liquid && direction.Value is Direction.Type.Up && !neighbor.Liquid;
                var light = neighborData.Light;
                EmitFace(
                    pos.ToNumerics(),
                    direction,
                    uvOffset,
                    spriteUnit,
                    light,
                    block.Transparent,
                    shortenY
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
            if (Persist && TransparentFaces.Count is not 0)
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

    public void Render(GL gl, Part part)
    {
        if (
            (part is Part.Transparent && TransparentFaces.Count is 0)
            || part is Part.Solid && SolidFaces.Count is 0
        )
            return;
        // TODO: shit here
        var shader = State.Renderer.Shaders[Renderer.ShaderType.Chunk];
        var model = Matrix4x4.CreateTranslation(Chunk.Position.ToNumerics());
        shader.UniformMatrix4(gl, "m", model);

        Vao.Link(gl, Vbo, 0, 3, VertexAttribPointerType.Float, 0);
        Vao.Link(gl, Vbo, 1, 2, VertexAttribPointerType.Float, sizeof(float) * 3);
        Vao.Link(gl, Vbo, 2, 4, VertexAttribPointerType.Float, sizeof(float) * 5);

        // TODO: shit here wireframe
        if (part is Part.Transparent)
            IboTransparent.DrawElements(gl, State.Renderer.Wireframe);
        else
            IboSolid.DrawElements(gl, State.Renderer.Wireframe);
    }

    public void SetPersist(bool persist)
    {
        if (Persist == persist)
            return;
        Persist = persist;
        if (!persist)
        {
            TransparentFaces = [];
            SolidFaces = [];
        }
    }
}
