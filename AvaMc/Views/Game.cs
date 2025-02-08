using System;
using System.Numerics;
using Avalonia.Input;
using AvaMc.Blocks;
using AvaMc.Coordinates;
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
        GlobalState.Renderer = new(gl);
        GlobalState.World = new(gl);
        // TODO: not good here
        GlobalState.Renderer.PerspectiveCamera.Position = new(0, 80, 0);
    }

    public void Delete(GL gl)
    {
        GlobalState.Renderer.Delete(gl);
        GlobalState.World.Delete(gl);
    }

    //TODO: for test
    static Random Random = new();

    public void Tick(GL gl)
    {
        GlobalState.Ticks++;
        GlobalState.World.Tick();
        var blockPosition = new BlockWorldPosition(GlobalState.World.Player.Camera.Position);
        GlobalState.World.SetCenter(gl, blockPosition);

        // TODO: for test
        if (GlobalState.Game.Keyboard[Key.C].PressedTick)
        {
            var r = Random.Next() % 16;
            var g = Random.Next() % 16;
            var b = Random.Next() % 16;
            Light.Add(GlobalState.World, blockPosition, new(15, 15, 15, 15, 0));
        }
        if (GlobalState.Game.Keyboard[Key.V].PressedTick)
        {
            Light.Remove(GlobalState.World, blockPosition);
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
        GlobalState.Renderer.Update();
        GlobalState.World.Update(gl);

        if (GlobalState.Game.Keyboard[Key.T].Pressed)
            GlobalState.Renderer.Wireframe = !GlobalState.Renderer.Wireframe;
    }

    float Z = 0;

    public void Render(GL gl)
    {
        GlobalState.Renderer.Prepare(gl, Renderer.RenderPass.Pass3D);
        GlobalState.World.Render(gl);

        // Z -= 0.05f;
        GlobalState.Renderer.Prepare(gl, Renderer.RenderPass.Pass2D);
        GlobalState.Renderer.PushCamera();
        {
            GlobalState.Renderer.SetCamera(CameraType.Orthographic);
            // TODO: not good here
            GlobalState.Renderer.OrthographicCamera.Initialize(
                Vector2.Zero,
                GlobalState.Game.WindowSize.ToVector2()
            );
            var pos = new Vector3(
                (float)(GlobalState.Game.WindowSize.Width / 2) - 8,
                (float)(GlobalState.Game.WindowSize.Height / 2) - 8,
                0
            );
            var size = new Vector3(16, 16, 0);
            var color = new Vector4(1.0f, 1.0f, 1.0f, 0.4f);
            var min = new Vector2(0, 0);
            var max = new Vector2(1, 1);
            GlobalState.Renderer.ImmediateQuad(
                gl,
                GlobalState.Renderer.Textures[Renderer.TextureType.CrossHair],
                pos,
                size,
                color,
                min,
                max
            );
        }
        GlobalState.Renderer.PopCamera();
    }
}
