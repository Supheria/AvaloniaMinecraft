using System.Collections.Generic;
using System.Numerics;
using AvaMc.Comparers;
using AvaMc.Gfx;
using AvaMc.Util;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public sealed partial class ChunkMesh
{
    // public enum MeshPart : byte
    // {
    //     Base,
    //     Transparent,
    // }

    Chunk Chunk { get; set; }
    public List<ChunkVertex> Vertices { get; set; } = [];
    public List<Face> Faces { get; set; } = [];
    public List<uint> Indices { get; set; } = [];
    uint VertexCount { get; set; }

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
    IboHandler Ibo { get; }

    public ChunkMesh(GL gl, Chunk chunk)
    {
        Chunk = chunk;
        Vao = VaoHandler.Create(gl);
        Vbo = VboHandler.Create(gl, false);
        Ibo = IboHandler.Create(gl, false);
    }

    public void Delete(GL gl)
    {
        Vao.Delete(gl);
        Vbo.Delete(gl);
        Ibo.Delete(gl);
    }

    // private static int DepthSortCompare(Vector3 center, Face face1, Face face2)
    // {
    //     var l1 = Vector3.Subtract(center, face1.Position).LengthSquared();
    //     var l2 = Vector3.Subtract(center, face2.Position).LengthSquared();
    //     return -Math.Sign(l1 - l2);
    // }

    private void DepthSort(Vector3 center)
    {
        // TODO: not good here
        foreach (var face in Faces)
        {
            face.DistanceSquared = Vector3.DistanceSquared(center, face.Position);
        }
        var comparer = new FaceDepthComparer(DepthOrder.Farther);
        Faces.Sort(comparer);

        var indices = new List<uint>();
        for (var i = 0; i < Faces.Count; i++)
        {
            var face = Faces[i];
            for (var j = 0; j < 6; j++)
                indices.Add(Indices[face.IndicesBase + j]);
            // TODO: don't know why here
            face.IndicesBase = i * 6;
        }
        Indices = indices;
    }

    public void Prepare()
    {
        Vertices = [];
        Faces = [];
        Indices = [];
        VertexCount = 0;
    }

    public void Finalize(GL gl, bool depthSort)
    {
        Vbo.Buffer(gl, Vertices);
        if (depthSort)
            DepthSort(Chunk.World.Player.Camera.Position);
        Ibo.Buffer(gl, Indices);
    }

    public void Render(GL gl)
    {
        var shader = State.Shader;
        shader.Use(gl);
        shader.UniformCamera(gl, State.World.Player.Camera);
        // shader.UniformCamera(gl, State.TestCamera);
        var model = Matrix4x4.CreateTranslation(Chunk.Position.ToNumerics());
        shader.UniformMatrix4(gl, "m", model);
        shader.UniformTexture(gl, "tex", State.BlockAtlas.Atlas.Texture);

        Vao.Link(gl, Vbo, 0, 3, VertexAttribPointerType.Float, 0);
        Vao.Link(gl, Vbo, 1, 2, VertexAttribPointerType.Float, sizeof(float) * 3);
        Vao.Link(gl, Vbo, 2, 3, VertexAttribPointerType.Float, sizeof(float) * 5);

        Vao.Bind(gl);
        Ibo.DrawElements(gl, State.Wireframe);
    }

    public void EmitSprite(Vector3 position, Vector2 uvOffset, Vector2 uvUnit)
    {
        for (var i = 0; i < 2; i++)
        {
            var face = new Face() { IndicesBase = Indices.Count + (i * 6), Position = position };
            Faces.Add(face);
        }

        for (var i = 0; i < 8; i++)
        {
            var index = i * 3;
            var x = ChunkData.CubeVertices[index++] + position.X;
            var y = ChunkData.CubeVertices[index++] + position.Y;
            var z = ChunkData.CubeVertices[index] + position.Z;
            index = (i % 4) * 2;
            var u = ChunkData.CubeUvs[index++] * uvUnit.X + uvOffset.X;
            var v = ChunkData.CubeUvs[index] * uvUnit.Y + uvOffset.Y;

            var color = 1f;
            var vertex = new ChunkVertex(new(x, y, z), new(u, v), new(color, color, color));
            Vertices.Add(vertex);
        }

        for (var i = 0; i < 12; i++)
        {
            var index = ChunkData.SpriteIndices[i] + VertexCount;
            Indices.Add(index);
        }

        VertexCount += 8;
    }

    public void EmitFace(
        Vector3 position,
        Direction direction,
        Vector2 uvOffset,
        Vector2 uvUnit,
        bool transparent,
        bool shortenY
    )
    {
        if (transparent)
        {
            var face = new Face() { IndicesBase = Indices.Count, Position = position };
            Faces.Add(face);
        }

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
            float color;
            if (transparent)
                color = 1;
            else
            {
                switch (direction.Value)
                {
                    case Direction.Type.Up:
                        color = 1;
                        break;
                    case Direction.Type.North:
                    case Direction.Type.South:
                        color = 0.86f;
                        break;
                    case Direction.Type.East:
                    case Direction.Type.West:
                        color = 0.8f;
                        break;
                    case Direction.Type.Down:
                        color = 0.6f;
                        break;
                    default:
                        color = 0;
                        break;
                }
            }

            var vertex = new ChunkVertex(new(x, y, z), new(u, v), new(color, color, color));
            Vertices.Add(vertex);
        }

        for (var i = 0; i < 6; i++)
        {
            var index = VertexCount + ChunkData.FaceIndices[i];
            Indices.Add(index);
        }

        VertexCount += 4;
    }
}
