using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using AvaMc.Util;
using Silk.NET.OpenGLES;
using Point = Avalonia.Point;

namespace AvaMc.Views;

public abstract class GlEsControl : OpenGlControlBase, ICustomHitTest
{
    public record FrameInfo(int Fps, int Tps)
    {
        public override string ToString()
        {
            return $"Fps:{Fps}, Tps:{Tps}";
        }
    }

    public EventHandler<FrameInfo>? FrameInfoUpdated;
    GL? Gl { get; set; }
    PixelSize ViewPortSize { get; set; }
    bool DoChangeViewPort { get; set; }
    DispatcherTimer Timer { get; } = new();
    long LastTime { get; set; }
    int FrameCount { get; set; }
    protected int Ticks { get; set; }

    // KeyEventArgs? KeyState { get; set; }
    // Vector2 PointerPostionDiff { get; set; }
    // protected Point LastPointerPostion { get; set; }
    protected double TimeDelta { get; set; }

    protected GlEsControl()
    {
        Timer.Interval = TimeSpan.FromMilliseconds(10);
        Timer.Tick += (_, _) => RequestNextFrameRendering();
        Timer.Start();
    }

    public bool HitTest(Point point)
    {
        return true;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var size = e.NewSize * scaling;
        var viewPortSize = new PixelSize((int)size.Width, (int)size.Height);
        if (ViewPortSize != viewPortSize)
        {
            DoChangeViewPort = true;
            ViewPortSize = viewPortSize;
        }
    }

    protected sealed override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);
        Gl = GL.GetApi(gl.GetProcAddress);
        OnGlInit(Gl);

        LastTime = Time.Now();
    }

    protected abstract void OnGlInit(GL gl);

    protected sealed override void OnOpenGlDeinit(GlInterface gl)
    {
        base.OnOpenGlDeinit(gl);
        if (Gl is null)
            return;
        OnGlDeinit(Gl);
        Gl?.Dispose();
    }

    protected abstract void OnGlDeinit(GL gl);

    protected sealed override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (Gl is null)
            return;
        CheckSettings(gl);
        OnGlRender(Gl);
        OnFrameUpdate();
    }

    private void CheckSettings(GlInterface gl)
    {
        if (DoChangeViewPort)
        {
            gl.Viewport(0, 0, ViewPortSize.Width, ViewPortSize.Height);
            DoChangeViewPort = false;
        }
    }

    protected abstract void OnGlRender(GL gl);

    private void OnFrameUpdate()
    {
        FrameCount++;
        var now = Time.Now();
        if (now - LastTime < Time.NanosecondsPerSecond)
            return;
        var info = new FrameInfo(FrameCount, Ticks);
        FrameInfoUpdated?.Invoke(this, info);
        FrameCount = 0;
        LastTime = now;
        Ticks = 0;
    }
}
