using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaMc.Extensions;
using AvaMc.Gfx;
using AvaMc.Util;
using Silk.NET.OpenGLES;

namespace AvaMc.Views;

public sealed class GameControl : GlEsControl
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

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed)
            State.Game.Pointer[PointerButton.Left].Down = true;
        if (point.Properties.IsRightButtonPressed)
            State.Game.Pointer[PointerButton.Right].Down = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        State.Game.Pointer[PointerButton.Left].Down = false;
        State.Game.Pointer[PointerButton.Right].Down = false;
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
        State.Shader = ShaderHandler.Create(gl, "basic");
        var texture = Texture2D.Create(gl, "blocks", 0);
        State.Atlas = Atlas.Create(texture, new(16, 16));
        State.World = new(gl);
        // State.TestCamera = Camera;
        // State.Wireframe = false;
        State.World.Player.Camera.Position = new(0, 66, 0);

        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);

        gl.Enable(EnableCap.CullFace);
        gl.CullFace(TriangleFace.Back);

        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

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
        Update(gl);

        gl.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        State.World.Render(gl);
    }

    private void Tick(GL gl)
    {
        State.Game.Pointer.Tick();
        State.Game.Keyboard.Tick();
        State.World.Tick();
        State.World.SetCenter(gl, State.World.Player.Camera.Position.CameraPosToBlockPos());
    }

    private void Update(GL gl)
    {
        State.Game.Pointer.Update();
        State.Game.Keyboard.Update();
        State.World.Update(gl);
        
        if (State.Game.Keyboard[Key.T].Pressed)
            State.Wireframe = !State.Wireframe;
    }
}
