using System;
using System.Numerics;
using Avalonia.Input;
using AvaMc.Blocks;
using AvaMc.Extensions;
using AvaMc.Gfx;
using AvaMc.Util;
using AvaMc.WorldBuilds;
using Silk.NET.OpenGLES;

namespace AvaMc.Views;

public sealed class Game
{
    public void Initialize(GL gl)
    {
        State.Renderer = new(gl);
        State.World = new(gl);
        // TODO: not good here
        State.Renderer.PerspectiveCamera.Position = new(0, 80, 0);
    }

    public void Delete(GL gl)
    {
        State.Renderer.Delete(gl);
        State.World.Delete(gl);
    }

    //TODO: for test
    static Random Random = new();

    public void Tick(GL gl)
    {
        State.Ticks++;
        State.World.Tick();
        State.World.SetCenter(gl, State.World.Player.Camera.Position.CameraPosToBlockPos());

        // TODO: for test
        if (State.Game.Keyboard[Key.C].PressedTick)
        {
            var pos = State.World.Player.Camera.Position.CameraPosToBlockPos();
            var r = Random.Next() % 16;
            var g = Random.Next() % 16;
            var b = Random.Next() % 16;
            Light.Add(State.World, pos, new(r, g, b, 15));
        }
        if (State.Game.Keyboard[Key.V].PressedTick)
        {
            var pos = State.World.Player.Camera.Position.CameraPosToBlockPos();
            Light.Remove(State.World, pos);
        }
        // if (State.Game.Keyboard[Key.C].PressedTick)
        // {
        //     for (var x = 0; x < 32; x++)
        //     {
        //         for (var y = 64; y < 80; y++)
        //         {
        //             State.World.SetBlockData(new(x, y, 4), new() { Id = BlockId.Glass });
        //             State.World.SetBlockData(new(x, y, 8), new() { Id = BlockId.Lava });
        //         }
        //         State.World.SetBlockData(new(40, 80, 4), new() { Id = BlockId.Rose });
        //     }
        // }
    }

    public void Update(GL gl)
    {
        State.Renderer.Update();
        State.World.Update(gl);

        if (State.Game.Keyboard[Key.T].Pressed)
            State.Renderer.Wireframe = !State.Renderer.Wireframe;
    }

    public void Render(GL gl)
    {
        State.Renderer.ClearColor = new(0.5f, 0.8f, 0.9f, 1.0f);
        State.Renderer.Prepare(gl, Renderer.RenderPass.Pass3D);
        State.World.Render(gl);

        State.Renderer.Prepare(gl, Renderer.RenderPass.Pass2D);
        State.Renderer.PushCamera();
        {
            State.Renderer.SetCamera(CameraType.Orthographic);
            var pos = new Vector3(
                (float)(State.Game.WindowSize.Width / 2) - 8,
                (float)(State.Game.WindowSize.Height / 2) - 8,
                0
            );
            var size = new Vector3(16, 16, 0);
            var color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
            var min = new Vector2(0, 0);
            var max = new Vector2(1, 1);
            State.Renderer.ImmediateQuad(
                gl,
                State.Renderer.Textures[Renderer.TextureType.CrossHair],
                pos,
                size,
                color,
                min,
                max
            );
        }
        State.Renderer.PopCamera();
    }
}
