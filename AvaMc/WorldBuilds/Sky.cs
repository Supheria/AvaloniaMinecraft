using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using AvaMc.Gfx;
using AvaMc.Util;
using Silk.NET.OpenGLES;

namespace AvaMc.WorldBuilds;

public sealed class Sky
{
    const int DayTicks = 600;
    const int NightTicks = 600;
    const int CycleTicks = DayTicks + NightTicks;
    const int SunChangeTicks = 100;
    const int HalfSunChangeTicks = SunChangeTicks / 2;

    private enum State
    {
        Day,
        Night,
        SunRise,
        SunSet,
    }

    private enum CelestialBody
    {
        Sun,
        Moon,
    }

    private enum Plane
    {
        Sky,
        Void,
    }

    static Dictionary<State, Vector4> SkyColors { get; } =
        new()
        {
            [State.Day] = ColorHelper.ValueToNormalizedRgba(0x87CEEBFF),
            [State.Night] = ColorHelper.ValueToNormalizedRgba(0x010422FF),
            [State.SunRise] = ColorHelper.ValueToNormalizedRgba(0xFFCA7CFF),
            [State.SunSet] = ColorHelper.ValueToNormalizedRgba(0xFFAB30FF),
        };
    static Dictionary<(State, Plane), Vector4> PlaneColors { get; } =
        new()
        {
            [(State.Day, Plane.Sky)] = ColorHelper.ValueToNormalizedRgba(0x87CEEBFF),
            [(State.Day, Plane.Void)] = ColorHelper.ValueToNormalizedRgba(0x87CEEBFF),
            [(State.Night, Plane.Sky)] = ColorHelper.ValueToNormalizedRgba(0x030832FF),
            [(State.Night, Plane.Void)] = ColorHelper.ValueToNormalizedRgba(0x030832FF),
        };

    private readonly struct Vertex
    {
        Vector3 Position { get; }
        Vector2 Uv { get; }

        public Vertex(float x, float y, float z, float u, float v)
        {
            Position = new(x, y, z);
            Uv = new(u, v);
        }
    }

    World World { get; }
    public float FogNear { get; set; }
    public float FogFar { get; set; }
    Vector4 FogColor { get; set; }
    public Vector4 ClearColor { get; set; }
    VaoHandler Vao { get; }
    VboHandler Vbo { get; }
    IboHandler Ibo { get; }

    public Sky(GL gl, World world)
    {
        World = world;
        Vao = VaoHandler.Create(gl);
        Vbo = VboHandler.Create(gl, false);
        Ibo = IboHandler.Create(gl, false);

        GlobalState.Renderer.ClearColor = SkyColors[State.Day];
        var vertices = new Vertex[]
        {
            new(-0.5f, -0.5f, 0.0f, 0.0f, 0.0f),
            new(-0.5f, +0.5f, 0.0f, 0.0f, 1.0f),
            new(+0.5f, +0.5f, 0.0f, 1.0f, 1.0f),
            new(+0.5f, -0.5f, 0.0f, 1.0f, 0.0f),
        };
        var indices = new uint[] { 3, 0, 1, 3, 1, 2 };
        Vbo.Buffer(gl, vertices);
        Ibo.Buffer(gl, indices);
    }

    public void Delete(GL gl)
    {
        Vao.Delete(gl);
        Vbo.Delete(gl);
        Ibo.Delete(gl);
    }

    private State GetState(long ticks)
    {
        ticks %= CycleTicks;
        if (ticks <= HalfSunChangeTicks)
            return State.SunRise;
        if (ticks <= DayTicks - HalfSunChangeTicks)
            return State.Day;
        if (ticks <= DayTicks + HalfSunChangeTicks)
            return State.SunSet;
        if (ticks <= CycleTicks - HalfSunChangeTicks)
            return State.Night;
        return State.SunRise;
    }

    private State GetNextState(State state)
    {
        return state switch
        {
            State.Day => State.SunSet,
            State.Night => State.SunRise,
            State.SunRise => State.Day,
            State.SunSet => State.Night,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
        };
    }

    private State GetPrevState(State state)
    {
        return state switch
        {
            State.Day => State.SunRise,
            State.Night => State.SunSet,
            State.SunRise => State.Night,
            State.SunSet => State.Day,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
        };
    }

    private float GetStateProgress(long ticks)
    {
        ticks %= CycleTicks;
        if (ticks <= HalfSunChangeTicks)
        {
            return 0.5f + (float)ticks / HalfSunChangeTicks;
        }
        if (ticks <= DayTicks - HalfSunChangeTicks)
        {
            return (float)(ticks - HalfSunChangeTicks) / (DayTicks - SunChangeTicks);
        }
        if (ticks <= DayTicks + HalfSunChangeTicks)
        {
            return (float)(ticks - (DayTicks - HalfSunChangeTicks)) / SunChangeTicks;
        }
        if (ticks <= CycleTicks - HalfSunChangeTicks)
        {
            return (float)(ticks - (DayTicks + HalfSunChangeTicks)) / (NightTicks - SunChangeTicks);
        }
        return (float)(ticks - (CycleTicks - HalfSunChangeTicks)) / SunChangeTicks;
    }

    private State GetDayOrNight(long ticks)
    {
        ticks %= CycleTicks;
        return (ticks >= 0 && ticks <= DayTicks) ? State.Day : State.Night;
    }

    private float GetDayOrNightProgress(long ticks)
    {
        ticks %= CycleTicks;
        return GetDayOrNight(ticks) switch
        {
            State.Day => (float)ticks / DayTicks,
            _ => (float)(ticks - DayTicks) / NightTicks,
        };
    }

    private static Matrix4x4 PlaneModel(Vector3 translation, Vector3 rotation, Vector3 scale)
    {
        var trans = Matrix4x4.CreateTranslation(translation);
        var rotX = Matrix4x4.CreateRotationX(rotation.X);
        var rotY = Matrix4x4.CreateRotationY(rotation.Y);
        var rotZ = Matrix4x4.CreateRotationZ(rotation.Z);
        var sca = Matrix4x4.CreateScale(scale);
        return sca * rotX * rotY * rotZ * trans;
    }

    private static Matrix4x4 CelestialModel(CelestialBody body, Vector3 center)
    {
        var ticks = GlobalState.Ticks % CycleTicks;
        var time = 0f;
        var show = true;
        switch (body)
        {
            case CelestialBody.Sun:
                time = (float)ticks / DayTicks;
                show = ticks >= 0 && ticks <= DayTicks;
                break;
            case CelestialBody.Moon:
                time = (float)(ticks - DayTicks) / NightTicks;
                show = ticks > DayTicks && ticks <= CycleTicks;
                break;
        }

        const float angleStart = -(MathF.PI + 0.4f);
        const float angleEnd = 0.4f;
        var angle = show
            ? MathF.IEEERemainder(
                MathF.Tau + (time * (angleEnd - angleStart) + angleStart),
                MathF.Tau
            )
            : -(MathF.PI + 1);

        var transY = Matrix4x4.CreateTranslation(Vector3.Add(center, new(0, 4, 0)));
        var rotX = Matrix4x4.CreateRotationX(angle);
        var transZ = Matrix4x4.CreateTranslation(new(0, 0, 10));
        var sca = Matrix4x4.CreateScale(new Vector3(8, 8, 0));
        return sca * transZ * rotX * transY;
    }

    private void RenderPlane(GL gl, Texture2D? texture, Vector4 color, Matrix4x4 model)
    {
        // TODO: shit here
        var shader = GlobalState.Renderer.Shaders[Renderer.ShaderType.Sky];
        shader.UniformMatrix4(gl, "m", model);
        shader.UniformVector4(gl, "color", color);
        if (texture is null)
            shader.UniformInt(gl, "use_tex", 0);
        else
        {
            shader.UniformInt(gl, "use_tex", 1);
            shader.UniformTexture(gl, "tex", texture);
        }

        Vao.Link(gl, Vbo, 0, 3, VertexAttribPointerType.Float, 0);
        Vao.Link(gl, Vbo, 1, 2, VertexAttribPointerType.Float, sizeof(float) * 3);
        Ibo.DrawElements(gl, false);
    }

    public void Render(GL gl)
    {
        var center = GlobalState.Renderer.PerspectiveCamera.Position;
        // disable depth writing so the sky is always overwritten by everything else
        gl.DepthMask(false);
        var t = GetStateProgress(GlobalState.Ticks);
        var fogColor = Vector4.Zero;
        var skyColor = Vector4.Zero;
        var voidColor = Vector4.Zero;

        var state = GetState(GlobalState.Ticks);
        switch (state)
        {
            case State.Day:
                fogColor = SkyColors[State.Day];
                skyColor = SkyColors[State.Day];
                voidColor = SkyColors[State.Day];
                break;
            case State.Night:
                fogColor = SkyColors[State.Night];
                skyColor = SkyColors[State.Night];
                voidColor = SkyColors[State.Night];
                break;
            case State.SunRise:
                fogColor = ColorHelper.RgbaLerp3(
                    SkyColors[State.Night],
                    SkyColors[State.SunRise],
                    SkyColors[State.Day],
                    t
                );
                skyColor = ColorHelper.RgbaLerp(
                    PlaneColors[(State.Night, Plane.Sky)],
                    PlaneColors[(State.Day, Plane.Sky)],
                    t
                );
                voidColor = ColorHelper.RgbaLerp3(
                    PlaneColors[(State.Day, Plane.Sky)],
                    SkyColors[(State.SunRise)],
                    PlaneColors[(State.Day, Plane.Sky)],
                    t
                );
                break;
            case State.SunSet:
                fogColor = ColorHelper.RgbaLerp3(
                    SkyColors[State.Day],
                    SkyColors[State.SunSet],
                    SkyColors[State.Night],
                    t
                );
                skyColor = ColorHelper.RgbaLerp(
                    PlaneColors[(State.Day, Plane.Sky)],
                    PlaneColors[(State.Night, Plane.Sky)],
                    t
                );
                voidColor = ColorHelper.RgbaLerp3(
                    PlaneColors[(State.Day, Plane.Void)],
                    SkyColors[State.SunSet],
                    PlaneColors[(State.Night, Plane.Void)],
                    t
                );
                break;
        }
        FogColor = fogColor;
        ClearColor = fogColor;

        GlobalState.Renderer.UseShader(gl, Renderer.ShaderType.Sky);
        GlobalState.Renderer.SetViewProject(gl);
        var shader = GlobalState.Renderer.Shaders[Renderer.ShaderType.Sky];
        shader.UniformVector4(gl, "fog_color", fogColor);
        shader.UniformFloat(gl, "fog_near", FogNear / 8.0f);
        shader.UniformFloat(gl, "fog_far", FogFar);

        var model = PlaneModel(
            Vector3.Add(center, new(0, 16, 0)),
            new(float.DegreesToRadians(-90), 0, 0),
            new(1024.0f, 1024.0f, 0)
        );
        RenderPlane(gl, null, skyColor, model);

        model = PlaneModel(
            Vector3.Add(center, new(0, -16, 0)),
            new(float.DegreesToRadians(90), 0, 0),
            new(1024.0f, 1024.0f, 0)
        );
        RenderPlane(gl, null, voidColor, model);

        var time = (GlobalState.Ticks % 600) / 600f;
        var angle = time * MathF.Tau;

        model = CelestialModel(CelestialBody.Sun, center);
        RenderPlane(
            gl,
            GlobalState.Renderer.Textures[Renderer.TextureType.Sun],
            Vector4.One,
            model
        );

        model = CelestialModel(CelestialBody.Moon, center);
        RenderPlane(
            gl,
            GlobalState.Renderer.Textures[Renderer.TextureType.Moon],
            Vector4.One,
            model
        );

        gl.DepthMask(true);
    }
}
