using System;
using Avalonia.Input;
using AvaMc.Util;
using Silk.NET.Maths;

namespace AvaMc.Views;

public class CameraTest : Camera
{
    float Speed { get; set; } = 2.0f * 10;
    float Sensitivity { get; set; } = 20.0f;
    Vector3D<float> Up { get; set; } = Vector3D<float>.UnitY;
    Vector3D<float> Front { get; set; } = -Vector3D<float>.UnitZ;
    Vector3D<float> Right { get; set; } = Vector3D<float>.UnitX;
    public Vector3D<float> Position { get; private set; } = Vector3D<float>.Zero;
    float PitchDegrees { get; set; }
    float YawDegrees { get; set; } = -90.0f;

    public void SetPosition(Vector3D<float> position)
    {
        Position = position;
        UpdateViewMatrix();
    }

    public void SetRotation(float pitchDegrees, float yawDegrees)
    {
        PitchDegrees = pitchDegrees;
        YawDegrees = yawDegrees;
        UpdateVectors();
        UpdateViewMatrix();
    }

    public void SetSize(Vector2D<float> size, float fovDegrees, float nearClipPlane, float farClipPlane)
    {
        Project = Matrix4X4.CreatePerspectiveFieldOfView(
            float.DegreesToRadians(fovDegrees),
            (float)(size.X / size.Y),
            nearClipPlane,
            farClipPlane
        );
    }

    private void UpdateViewMatrix()
    {
        View = Matrix4X4.CreateLookAt(Position, Position + Front, Up);
    }

    public void UpdateControl(
        KeyEventArgs? keyState,
        Vector2D<float> pointerPostionDiff,
        float timeDelta
    )
    {
        if (keyState is not null)
        {
            switch (keyState.Key)
            {
                case Key.W:
                    Position += Front * Speed * timeDelta;
                    UpdateViewMatrix();
                    break;
                case Key.S:
                    Position -= Front * Speed * timeDelta;
                    UpdateViewMatrix();
                    break;
                case Key.A:
                    Position -= Right * Speed * timeDelta;
                    UpdateViewMatrix();
                    break;
                case Key.D:
                    Position += Right * Speed * timeDelta;
                    UpdateViewMatrix();
                    break;
                case Key.Space:
                    Position = new(Position.X, Position.Y + Speed * timeDelta, Position.Z);
                    UpdateViewMatrix();
                    break;
                case Key.LeftShift:
                    Position = new(Position.X, Position.Y - Speed * timeDelta, Position.Z);
                    UpdateViewMatrix();
                    break;
            }
        }
        if (pointerPostionDiff != Vector2D<float>.Zero)
        {
            YawDegrees += pointerPostionDiff.X * Sensitivity * timeDelta;
            PitchDegrees -= pointerPostionDiff.Y * Sensitivity * timeDelta;
            UpdateVectors();
            UpdateViewMatrix();
        }
    }

    private void UpdateVectors()
    {
        PitchDegrees =
            PitchDegrees > 89.0f ? 89.0f
            : PitchDegrees < -89.0f ? -89.0f
            : PitchDegrees;
        var x =
            MathF.Cos(float.DegreesToRadians(PitchDegrees))
            * MathF.Cos(float.DegreesToRadians(YawDegrees));
        var y = MathF.Sin(float.DegreesToRadians(PitchDegrees));
        var z =
            MathF.Cos(float.DegreesToRadians(PitchDegrees))
            * MathF.Sin(float.DegreesToRadians(YawDegrees));
        Front = Vector3D.Normalize<float>(new(x, y, z));
        Right = Vector3D.Normalize(Vector3D.Cross(Front, Vector3D<float>.UnitY));
        Up = Vector3D.Normalize(Vector3D.Cross(Right, Front));
    }
}
