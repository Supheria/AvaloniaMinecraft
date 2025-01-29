using System.Collections.Generic;
using AvaMc.Gfx;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public sealed class ChunkMesh
{
    public enum MeshPart : byte
    {
        Base,
        Transparent,
    }

    Chunk Chunk { get; set; }
    List<uint> Indices { get; } = [];
    List<Face> Faces { get; } = [];

    // TODO: vertex
    // List<Vertex> Vertices { get; } = [];

    public int VertexCount { get; set; }

    // if true, this mesh needs to be finalized (uploaded)
    public bool Finalize { get; set; } = true;

    // if true, this mesh will be rebuilt next time it is rendered
    public bool Dirty { get; set; } = true;

    // if true, this mesh will be depth sorted next time it is rendered
    public bool DepthSort { get; set; } = true;

    // if true, this mesh will be destroyed when its data is next accessible
    public bool Destroy { get; set; } = true;

    // if true, index and face buffers are kept in memory after building
    public bool Persist { get; set; } = true;
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
}
