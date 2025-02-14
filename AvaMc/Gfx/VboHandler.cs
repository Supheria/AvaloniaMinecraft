using System;
using System.Collections.Generic;
using System.Linq;
using Hexa.NET.Utilities;
using Silk.NET.OpenGLES;

namespace AvaMc.Gfx;

public struct VboHandler
{
    uint Handle { get; }
    bool Dynamic { get; }
    public uint Stride { get; private set; }

    private VboHandler(uint handle, bool dynamic)
    {
        Handle = handle;
        Dynamic = dynamic;
    }

    public static VboHandler Create(GL gl, bool dynamic)
    {
        var handle = gl.GenBuffer();
        var vbo = new VboHandler(handle, dynamic);
        return vbo;
    }
    
    public static unsafe VboHandler* CreatePointer(GL gl, bool dynamic)
    {
        var p = Utils.AllocT<VboHandler>(1);
        var handle = gl.GenBuffer();
        *p = new VboHandler(handle, dynamic);
        return p;
    }

    public unsafe void Buffer<T>(GL gl, ReadOnlySpan<T> data)
        where T : unmanaged
    {
        Bind(gl);
        var stride = sizeof(T);
        gl.BufferData(
            BufferTargetARB.ArrayBuffer,
            (uint)(stride * data.Length),
            data,
            Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StaticDraw
        );
        Stride = (uint)stride;
    }

    public void Bind(GL gl)
    {
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, Handle);
    }

    public void Unbind(GL gl)
    {
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }

    public void Delete(GL gl)
    {
        gl.DeleteBuffer(Handle);
    }
    
    public static unsafe void Release(GL gl, VboHandler* p)
    {
        p->Delete(gl);
        Utils.Free(p);
    }
}
