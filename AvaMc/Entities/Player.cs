using System.Diagnostics;
using System.Numerics;
using Avalonia.Input;
using AvaMc.Extensions;
using AvaMc.Util;
using AvaMc.WorldBuilds;
using Microsoft.Xna.Framework;
using Silk.NET.OpenGLES;

namespace AvaMc.Entities;

// TODO
public class Player
{
    const float MouseSensitivity = 3.2f;
    const float Speed = 0.24f;
    World World { get; set; }
    public PerspectiveCamera Camera { get; set; }
    bool HasLookBlock { get; set; }
    Vector3I LookBlock { get; set; }
    Direction LookFace { get; set; }

    public Player(World world)
    {
        World = world;
        Camera = new();
        Camera.Initialize(float.DegreesToRadians(75));
    }

    public void Delete(GL gl) { }

    public void Render(GL gl) { }

    public void Update()
    {
        Camera.Update();
        Camera.Pitch -=
            State.Game.Pointer.Delta.Y / (State.Game.FrameDelta / (MouseSensitivity * 10000));
        Camera.Yaw -=
            State.Game.Pointer.Delta.X / (State.Game.FrameDelta / (MouseSensitivity * 10000));
        State.Game.Pointer.Delta = Vector2.Zero;
    }

    public void Tick()
    {
        var forward = new Vector3(float.Sin(Camera.Yaw), 0, float.Cos(Camera.Yaw));
        var right = Vector3.Cross(Vector3.UnitY, forward);

        var direction = Vector3.Zero;
        var keyboard = State.Game.Keyboard;
        if (keyboard[Key.W].Down)
            direction = Vector3.Add(direction, forward);
        if (keyboard[Key.S].Down)
            direction = Vector3.Subtract(direction, forward);
        if (keyboard[Key.A].Down)
            direction = Vector3.Add(direction, right);
        if (keyboard[Key.D].Down)
            direction = Vector3.Subtract(direction, right);
        if (keyboard[Key.Space].Down)
            direction = Vector3.Add(direction, Vector3.UnitY);
        if (keyboard[Key.LeftShift].Down)
            direction = Vector3.Subtract(direction, Vector3.UnitY);
        direction = Vector3.Normalize(direction);
        Vector3 movement;
        if (direction.IsNaN())
            movement = Vector3.Zero;
        else
            movement = Vector3.Multiply(direction, Speed);
        Camera.Position = Vector3.Add(Camera.Position, movement);

        // TODO: look at block
    }
}
