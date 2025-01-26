using System.Collections.Generic;
using System.Linq;
using Silk.NET.OpenGLES;

namespace AvaMc.Gfx;

public sealed unsafe class Vbo<T> : Resource
    where T : unmanaged
{
    public uint Stride { get; }

    public Vbo(GL gl, ICollection<T> data, bool dynamic)
    {
        Handle = gl.GenBuffer();
        Bind(gl);
        var array = data.ToArray();
        var stride = sizeof(T);
        gl.BufferData<T>(
            BufferTargetARB.ArrayBuffer,
            (uint)(stride * array.Length),
            data.ToArray(),
            dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StaticDraw
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
}
