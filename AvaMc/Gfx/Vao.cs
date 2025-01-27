using System;
using Silk.NET.OpenGLES;

namespace AvaMc.Gfx;

public sealed class Vao : Resource
{
    public Vao(GL gl)
    {
        Handle = gl.GenVertexArray();
    }

    public void Link<T>(GL gl, Vbo<T> vbo, uint slot, int count, GLEnum type, int offset)
        where T : unmanaged
    {
        Bind(gl);
        vbo.Bind(gl);
        switch (type)
        {
            case GLEnum.Float:
                gl.VertexAttribPointer(slot, count, type, false, vbo.Stride, offset);
                break;
            case GLEnum.UnsignedInt:
                gl.VertexAttribIPointer(slot, count, type, vbo.Stride, offset);
                break;
            default:
                throw new ArgumentOutOfRangeException($"unsupported vertex attribute type");
        }
    }

    public void Bind(GL gl)
    {
        gl.BindVertexArray(Handle);
    }

    public void Unbind(GL gl)
    {
        gl.BindVertexArray(0);
    }

    public void Delete(GL gl)
    {
        gl.DeleteVertexArray(Handle);
    }
}
