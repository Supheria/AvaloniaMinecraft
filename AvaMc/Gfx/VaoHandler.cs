using System;
using Silk.NET.OpenGLES;

namespace AvaMc.Gfx;

public readonly struct VaoHandler
{
    uint Handle { get; }

    private VaoHandler(uint handle)
    {
        Handle = handle;
    }

    public static VaoHandler Create(GL gl)
    {
        var handle = gl.GenVertexArray();
        return new(handle);
    }

    public void Link(
        GL gl,
        VboHandler vbo,
        uint slot,
        int count,
        VertexAttribPointerType type,
        int offset
    )
    {
        Bind(gl);
        vbo.Bind(gl);
        if (type is not VertexAttribPointerType.Float)
            throw new ArgumentOutOfRangeException($"unsupported vertex attribute type");
        gl.VertexAttribPointer(slot, count, type, false, vbo.Stride, offset);
        gl.EnableVertexAttribArray(slot);
    }

    public unsafe void Link(
        GL gl,
        VboHandler* vbo,
        uint slot,
        int count,
        VertexAttribPointerType type,
        int offset
    )
    {
        Bind(gl);
        vbo->Bind(gl);
        if (type is not VertexAttribPointerType.Float)
            throw new ArgumentOutOfRangeException($"unsupported vertex attribute type");
        gl.VertexAttribPointer(slot, count, type, false, vbo->Stride, offset);
        gl.EnableVertexAttribArray(slot);
    }

    public void Link(
        GL gl,
        VboHandler vbo,
        uint slot,
        int count,
        VertexAttribPointerType type,
        uint stride,
        int offset
    )
    {
        Bind(gl);
        vbo.Bind(gl);
        if (type is not VertexAttribPointerType.Float)
            throw new ArgumentOutOfRangeException($"unsupported vertex attribute type");
        gl.VertexAttribPointer(slot, count, type, false, stride, offset);
        gl.EnableVertexAttribArray(slot);
    }

    public void Link(
        GL gl,
        VboHandler vbo,
        uint slot,
        int count,
        VertexAttribIType type,
        int offset
    )
    {
        Bind(gl);
        vbo.Bind(gl);
        if (type is not VertexAttribIType.UnsignedInt)
            throw new ArgumentOutOfRangeException($"unsupported vertex attribute type");
        gl.VertexAttribIPointer(slot, count, type, vbo.Stride, offset);
        gl.EnableVertexAttribArray(slot);
    }

    public unsafe void Link(
        GL gl,
        VboHandler* vbo,
        uint slot,
        int count,
        VertexAttribIType type,
        int offset
    )
    {
        Bind(gl);
        vbo->Bind(gl);
        if (type is not VertexAttribIType.UnsignedInt)
            throw new ArgumentOutOfRangeException($"unsupported vertex attribute type");
        gl.VertexAttribIPointer(slot, count, type, vbo->Stride, offset);
        gl.EnableVertexAttribArray(slot);
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
