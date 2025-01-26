using System.Collections.Generic;
using System.Linq;
using Silk.NET.OpenGLES;

namespace AvaMc.Gfx;

public sealed class Ibo : Resource
{
    uint ElementCount { get; }

    public Ibo(GL gl, ICollection<uint> data, bool dynamic)
    {
        Handle = gl.GenBuffer();
        Bind(gl);
        var array = data.ToArray();
        gl.BufferData<uint>(
            BufferTargetARB.ElementArrayBuffer,
            (uint)(sizeof(uint) * array.Length),
            data.ToArray(),
            dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StaticDraw
        );
        ElementCount = (uint)array.Length;
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

    public unsafe void DrawElements(GL gl, PrimitiveType renderMode)
    {
        gl.DrawElements(renderMode, ElementCount, DrawElementsType.UnsignedInt, null);
    }
}
