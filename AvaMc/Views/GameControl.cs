using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using AvaMc.Gfx;
using AvaMc.Util;
using AvaMc.WorldBuilds;
using Silk.NET.OpenGLES;

namespace AvaMc.Views;

public class GameControl : GlEsControl
{
    long LastFrameTime { get; set; }
    long TickRemainder { get; set; }
    long FrameDelta { get; set; }
    Point LastPointerPostion { get; set; }

    public GameControl()
    {
        KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown, handledEventsToo: true);
        KeyUpEvent.AddClassHandler<TopLevel>(OnKeyUp, handledEventsToo: true);
    }

    private void OnKeyDown(TopLevel _, KeyEventArgs e)
    {
        State.Game.Keyboard[e.Key].Down = true;
    }

    private void OnKeyUp(TopLevel _, KeyEventArgs e)
    {
        State.Game.Keyboard[e.Key].Down = false;
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        LastPointerPostion = e.GetPosition(this);
    }
    

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var position = e.GetPosition(this);
        var dX = position.X - LastPointerPostion.X;
        var dY = position.Y - LastPointerPostion.Y;
        State.Game.Pointer.Delta = new((float)dX, (float)dY);
        LastPointerPostion = position;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var size = e.NewSize * scaling;
        State.Game.WindowSize = size;
    }

    protected override void OnGlInit(GL gl)
    {
        // csharpier-ignore
        State.Shader = ShaderHandler.Create(gl, "basic", new()
        {
            [0] = "position",
            [1] = "uv",
            [2] = "color"
        });
        var texture = Texture2D.Create(gl, "blocks", 0);
        State.Atlas = Atlas.Create(texture, new(16, 16));
        State.World = new(gl);
        // State.TestCamera = Camera;
        // State.Wireframe = false;
        // State.World.Player.Camera.Position = new(0, 1, 0);

        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);

        // gl.Enable(EnableCap.CullFace);
        // gl.CullFace(TriangleFace.Back);

        // gl.Enable(EnableCap.Blend);
        // gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcColor);

        LastFrameTime = Time.Now();
    }

    protected override void OnGlDeinit(GL gl)
    {
        State.Shader.Delete(gl);
        State.World.Delete(gl);
    }

    protected override void OnGlRender(GL gl)
    {
        var now = Time.Now();
        State.Game.FrameDelta = FrameDelta = now - LastFrameTime;
        LastFrameTime = now;

        const long nsPerTick = Time.NanosecondsPerSecond / 60;
        var tick = FrameDelta + TickRemainder;
        while (tick > nsPerTick)
        {
            Tick(gl);
            tick -= nsPerTick;
        }
        TickRemainder = Math.Max(tick, 0);
        Update();

        gl.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        State.World.Render(gl);
    }

    private void Tick(GL gl)
    {
        State.Game.Pointer.Tick();
        State.Game.Keyboard.Tick();
        State.World.Tick();
        State.World.SetCenter(gl, Chunk.WorldPosToBlockPos(State.World.Player.Camera.Position));
    }

    private void Update()
    {
        State.World.Update();
    }
}
