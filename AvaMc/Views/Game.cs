using Avalonia.Input;
using AvaMc.Blocks;
using AvaMc.Extensions;
using AvaMc.Gfx;
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
    
    public void Tick(GL gl)
    {
        State.World.Tick();
        State.World.SetCenter(gl, State.World.Player.Camera.Position.CameraPosToBlockPos());
        State.Ticks++;
        
        // TODO: for test
        if (State.Game.Keyboard[Key.C].PressedTick)
        {
            for (var x = 0; x < 32; x++)
            {
                for (var y = 64; y < 80; y++)
                {
                    State.World.SetBlockData(new(x, y, 4), new() { BlockId = BlockId.Glass });
                    State.World.SetBlockData(new(x, y, 8), new() { BlockId = BlockId.Lava });
                }
                State.World.SetBlockData(new(40, 80, 4), new() { BlockId = BlockId.Rose });
            }
        }
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
    }
}