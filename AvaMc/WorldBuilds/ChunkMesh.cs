using System.Collections.Generic;
using System.Numerics;
using AvaMc.Extensions;
using AvaMc.Gfx;
using AvaMc.Util;
using Microsoft.Xna.Framework;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public sealed class ChunkMesh
{
    // public enum MeshPart : byte
    // {
    //     Base,
    //     Transparent,
    // }

    Chunk Chunk { get; set; }

    // TODO: vertex
    List<ChunkVertex> Vertices { get; } = [];
    List<uint> Indices { get; } = [];

    // List<Face> Faces { get; } = [];
    int IndexCount { get; set; }
    uint VertexCount { get; set; }

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

    public void Prepare()
    {
        VertexCount = 0;
        // IndexCount = 0;
    }
    
    public void Finalize(GL gl)
    {
        Vbo.Buffer(gl, Vertices);
        Ibo.Buffer(gl, Indices);
    }
    
    public void EmitFace(
        Direction direction)
    {
        for (var i = 0; i < 4; i++)
        {
            var index =
                ChunkMeshData.CubeIndices[(direction * 6) + ChunkMeshData.UniqueIndices[i]] * 3;
            var x = ChunkMeshData.CubeVertices[index++];
            var y = ChunkMeshData.CubeVertices[index++];
            var z =  ChunkMeshData.CubeVertices[index];
            index = i * 2;
            // var u = uvOffset.X + (uvUnit.X * ChunkMeshData.CubeUvs[index++]);
            // var v = uvOffset.Y + (uvUnit.Y * ChunkMeshData.CubeUvs[index]);
            var vertex = new ChunkVertex(new(x, y, z), new());
            Vertices.Add(vertex);
        }
        
        for (var i = 0; i < 6; i ++)
        {
            var index = VertexCount + ChunkMeshData.FaceIndices[i];
            Indices.Add(index);
        }
        
        VertexCount += 4;
    }

    public void EmitFace(
        Vector3I position,
        Direction direction,
        Vector2 uvOffset,
        Vector2 uvUnit
        // bool transparent,
        // bool shortenY
    )
    {
        // if (transparent)
        // {
        //     var face = new Face() { IndicesBase = (uint)Indices.Count, Position = position };
        //     Faces.Add(face);
        // }

        for (var i = 0; i < 4; i++)
        {
            var index =
                ChunkMeshData.CubeIndices[(direction * 6) + ChunkMeshData.UniqueIndices[i]] * 3;
            var x = position.X + ChunkMeshData.CubeVertices[index++];
            var y = position.Y + ChunkMeshData.CubeVertices[index++];
            var z = position.Z + ChunkMeshData.CubeVertices[index];
            index = i * 2;
            var u = uvOffset.X + (uvUnit.X * ChunkMeshData.CubeUvs[index++]);
            var v = uvOffset.Y + (uvUnit.Y * ChunkMeshData.CubeUvs[index]);
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

            var vertex = new ChunkVertex(new(x, y, z), new(u, v));
            Vertices.Add(vertex);
        }
        
        for (var i = 0; i < 6; i ++)
        {
            var index = VertexCount + ChunkMeshData.FaceIndices[i];
            Indices.Add(index);
        }
        
        VertexCount += 4;
    }
    
    public void Render(GL gl)
    {
        var shader = State.Shader;
        shader.Use(gl);
        shader.UniformCamera(gl, State.World.Player.Camera);
        // shader.UniformCamera(gl, State.TestCamera);
        var model = Matrix4x4.CreateTranslation(Chunk.Position.ToNumerics()); 
        shader.UniformMatrix4(gl, "m", model);
        shader.UniformTexture(gl, "tex", State.Atlas.Texture);
        
        Vao.Link(gl, Vbo, 0, 3, VertexAttribPointerType.Float, 0);
        Vao.Link(gl, Vbo, 1, 2, VertexAttribPointerType.Float, sizeof(float) * 3);
        
        Vao.Bind(gl);
        Ibo.DrawElements(gl, State.Wireframe);
    }
}
