// using System;
// using System.Numerics;
// using Avalonia.Input;
// using AvaMc.Util;
//
// namespace AvaMc.Views;
//
// public class CameraTest : Camera
// {
//     float Speed { get; set; } = 2.0f * 10;
//     float Sensitivity { get; set; } = 20.0f;
//     Vector3 Up { get; set; } = Vector3.UnitY;
//     Vector3 Front { get; set; } = -Vector3.UnitZ;
//     Vector3 Right { get; set; } = Vector3.UnitX;
//     public Vector3 Position { get; private set; } = Vector3.Zero;
//     float PitchDegrees { get; set; }
//     float YawDegrees { get; set; } = -90.0f;
//
//     public void SetPosition(Vector3 position)
//     {
//         Position = position;
//         UpdateViewMatrix();
//     }
//
//     public void SetRotation(float pitchDegrees, float yawDegrees)
//     {
//         PitchDegrees = pitchDegrees;
//         YawDegrees = yawDegrees;
//         UpdateVectors();
//         UpdateViewMatrix();
//     }
//
//     public void SetSize(Vector2 size, float fovDegrees, float nearClipPlane, float farClipPlane)
//     {
//         Project = Matrix4x4.CreatePerspectiveFieldOfView(
//             float.DegreesToRadians(fovDegrees),
//             (float)(size.X / size.Y),
//             nearClipPlane,
//             farClipPlane
//         );
//     }
//
//     private void UpdateViewMatrix()
//     {
//         View = Matrix4x4.CreateLookAt(Position, Position + Front, Up);
//     }
//
//     public void UpdateControl(
//         KeyEventArgs? keyState,
//         Vector2 pointerPostionDiff,
//         float timeDelta
//     )
//     {
//         if (keyState is not null)
//         {
//             switch (keyState.Key)
//             {
//                 case Key.W:
//                     Position += Front * Speed * timeDelta;
//                     UpdateViewMatrix();
//                     break;
//                 case Key.S:
//                     Position -= Front * Speed * timeDelta;
//                     UpdateViewMatrix();
//                     break;
//                 case Key.A:
//                     Position -= Right * Speed * timeDelta;
//                     UpdateViewMatrix();
//                     break;
//                 case Key.D:
//                     Position += Right * Speed * timeDelta;
//                     UpdateViewMatrix();
//                     break;
//                 case Key.Space:
//                     Position = new(Position.X, Position.Y + Speed * timeDelta, Position.Z);
//                     UpdateViewMatrix();
//                     break;
//                 case Key.LeftShift:
//                     Position = new(Position.X, Position.Y - Speed * timeDelta, Position.Z);
//                     UpdateViewMatrix();
//                     break;
//             }
//         }
//         if (pointerPostionDiff != Vector2.Zero)
//         {
//             YawDegrees += pointerPostionDiff.X * Sensitivity * timeDelta;
//             PitchDegrees -= pointerPostionDiff.Y * Sensitivity * timeDelta;
//             UpdateVectors();
//             UpdateViewMatrix();
//         }
//     }
//
//     private void UpdateVectors()
//     {
//         PitchDegrees =
//             PitchDegrees > 89.0f ? 89.0f
//             : PitchDegrees < -89.0f ? -89.0f
//             : PitchDegrees;
//         var x =
//             MathF.Cos(float.DegreesToRadians(PitchDegrees))
//             * MathF.Cos(float.DegreesToRadians(YawDegrees));
//         var y = MathF.Sin(float.DegreesToRadians(PitchDegrees));
//         var z =
//             MathF.Cos(float.DegreesToRadians(PitchDegrees))
//             * MathF.Sin(float.DegreesToRadians(YawDegrees));
//         Front = Vector3.Normalize(new(x, y, z));
//         Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
//         Up = Vector3.Normalize(Vector3.Cross(Right, Front));
//     }
// }
