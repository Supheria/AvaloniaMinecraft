using System;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.OpenGLES;

namespace AvaMc.Gfx;

public struct IboHandler
{
    uint Handle { get; }
    bool Dynamic { get; }
    uint ElementCount { get; set; }

    private IboHandler(uint handle, bool dynamic)
    {
        Handle = handle;
        Dynamic = dynamic;
    }

    public static IboHandler Create(GL gl, bool dynamic)
    {
        var handle = gl.GenBuffer();
        var ibo = new IboHandler(handle, dynamic);
        return ibo;
    }

    public void Buffer(GL gl, ReadOnlySpan<uint> data)
    {
        Bind(gl);
        ElementCount = (uint)data.Length;
        gl.BufferData(
            BufferTargetARB.ElementArrayBuffer,
            (uint)(sizeof(uint) * data.Length),
            data,
            Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StaticDraw
        );
    }

    public void Bind(GL gl)
    {
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Handle);
    }

    public void Unbind(GL gl)
    {
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    public void Delete(GL gl)
    {
        gl.DeleteBuffer(Handle);
    }

    public unsafe void DrawElements(GL gl, bool wireframe)
    {
        Bind(gl);
        gl.DrawElements(
            wireframe ? PrimitiveType.Lines : PrimitiveType.Triangles,
            ElementCount,
            DrawElementsType.UnsignedInt,
            null
        );
    }
}
