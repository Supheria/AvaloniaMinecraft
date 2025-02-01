using Silk.NET.Maths;

namespace AvaMc.Util;

public abstract class Camera
{
    public Matrix4X4<float> View { get; protected set; } = Matrix4X4<float>.Identity;
    public Matrix4X4<float> Project { get; protected set; } = Matrix4X4<float>.Identity;
}
