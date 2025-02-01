using System;
using Avalonia.Threading;
using AvaMc.Gfx;
using Silk.NET.OpenGLES;

namespace AvaMc.Views;

public class GameControl : TestControl
{
    DispatcherTimer Timer { get; } = new();

    public GameControl()
    {
        // KeyDownEvent.AddClassHandler<TopLevel>((_, e) => KeyState = e, handledEventsToo: true);
        // KeyUpEvent.AddClassHandler<TopLevel>((_, _) => KeyState = null, handledEventsToo: true);
        // TimeSpan.
        // Timer.Interval = new TimeSpan()
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
        State.TestCamera = Camera;
        // State.Wireframe = false;
        // State.World.Player.Camera.Position = new(0, 1, 0);

        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);

        // gl.Enable(EnableCap.CullFace);
        // gl.CullFace(TriangleFace.Back);

        // gl.Enable(EnableCap.Blend);
        // gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcColor);
    }

    // protected override void OnPointerEntered(PointerEventArgs e)
    // {
    //     base.OnPointerEntered(e);
    //     LastPointerPostion = e.GetPosition(this);
    // }
    //
    // protected override void OnPointerMoved(PointerEventArgs e)
    // {
    //     base.OnPointerMoved(e);
    //     var position = e.GetPosition(this);
    //     var dX = position.X - LastPointerPostion.X;
    //     var dY = position.Y - LastPointerPostion.Y;
    //     PointerPostionDiff = new((float)dX, (float)dY);
    //     LastPointerPostion = position;
    // }

    protected override void OnGlDeinit(GL gl)
    {
        State.Shader.Delete(gl);
        State.World.Delete(gl);
    }

    protected override void OnGlRender(GL gl)
    {
        gl.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        State.World.Render(gl);
    }
}
