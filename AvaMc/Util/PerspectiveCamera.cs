using System;
using System.Numerics;
using Avalonia;

namespace AvaMc.Util;

public sealed class PerspectiveCamera : Camera
{
    const float PiOver2 = float.Pi / 2;
    const float TwoPi = float.Pi * 2;
    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }
    public Vector3 Up { get; set; }
    public Vector3 Right { get; set; }
    public float Pitch
    {
        get => _pitch;
        set => _pitch = float.Clamp(value, -PiOver2, PiOver2);
    }
    float _pitch;
    public float Yaw
    {
        get => _yaw;
        set => _yaw = (value < 0f ? TwoPi : 0f) + MathF.IEEERemainder(value, TwoPi);
    }
    float _yaw;
    public float Fov { get; set; }
    public float ZNear { get; set; } = 0.01f;
    public float ZFar { get; set; } = 1000.0f;

    public void Initialize(float fov)
    {
        Fov = fov;
        Update();
    }

    public void Update()
    {
        var pitch = Pitch;
        var yaw = Yaw;
        var pos = Position;
        var fov = Fov;
        var ratio = (float)State.WindowSize.AspectRatio;
        var zNear = ZNear;
        var zFar = ZFar;

        var x = MathF.Cos(pitch) * MathF.Sin(yaw);
        var y = MathF.Sin(pitch);
        var z = MathF.Cos(pitch) * MathF.Cos(yaw);
        Direction = new(x, y, z);
        Right = Vector3.Cross(Vector3.UnitY, Direction);
        Up = Vector3.Cross(Direction, Right);
        View = Matrix4x4.CreateLookAt(pos, Vector3.Add(pos, Direction), Up);
        Project = Matrix4x4.CreatePerspectiveFieldOfView(fov, ratio, zNear, zFar);
    }
}
