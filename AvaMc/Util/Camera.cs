using System.Numerics;

namespace AvaMc.Util;

public abstract class Camera
{
    public Matrix4x4 View { get; protected set; } = Matrix4x4.Identity;
    public Matrix4x4 Project { get; protected set; } = Matrix4x4.Identity;
}
