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
    const int DayTicks = GlobalState.TickRate * 60 * 14;
    const int NightTicks = GlobalState.TickRate * 60 * 10;
    public const int CycleTicks = DayTicks + NightTicks;
    const int SunChangeTicks = 45 * GlobalState.TickRate;
    const int HalfSunChangeTicks = SunChangeTicks / 2;

    private enum SkyState : byte
    {
        Day = 0,
        Night,
        SunRise,
        SunSet,
    }

    private enum CelestialBody : byte
    {
        Sun,
        Moon,
    }

    private enum Plane : byte
    {
        Sky,
        Fog,
        Void,
    }

    static Dictionary<(SkyState, Plane), Vector4> SkyColors { get; } =
        new()
        {
            [(SkyState.Day, Plane.Sky)] = Rgba(0x87CEEBFF),
            [(SkyState.Day, Plane.Fog)] = Rgba(0x87CEEBFF),
            [(SkyState.Day, Plane.Void)] = Rgba(0x87CEEBFF),
            [(SkyState.Night, Plane.Sky)] = Rgba(0x020206FF),
            [(SkyState.Night, Plane.Fog)] = Rgba(0x010104FF),
            [(SkyState.Night, Plane.Void)] = Rgba(0x000000FF),
            [(SkyState.SunRise, Plane.Sky)] = Rgba(0xFFCA7CFF),
            [(SkyState.SunRise, Plane.Fog)] = Rgba(0xFFCA7CFF),
            [(SkyState.SunRise, Plane.Void)] = Rgba(0x000000FF),
            [(SkyState.SunSet, Plane.Sky)] = Rgba(0xFFAB30FF),
            [(SkyState.SunSet, Plane.Fog)] = Rgba(0xFFAB30FF),
            [(SkyState.SunSet, Plane.Void)] = Rgba(0x000000FF),
        };
    static Dictionary<SkyState, Vector4> SunlightColors { get; } =
        new() { [SkyState.Day] = Rgba(0xFFFFFFFF), [SkyState.Night] = Rgba(0x000000FF) };
    static Dictionary<SkyState, Vector4> CloudColors { get; } =
        new() { [SkyState.Day] = Rgba(0xFFFFFFFF), [SkyState.Night] = Rgba(0x111111FF) };

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
    public Vector4 SunlgihtColor { get; set; }
    SkyState State { get; set; }
    SkyState StateDayNight { get; set; }
    float StateProgress { get; set; }
    float DayNightProgress { get; set; }
    VaoHandler Vao { get; }
    VboHandler Vbo { get; }
    IboHandler Ibo { get; }

    public Sky(GL gl, World world)
    {
        World = world;
        Vao = VaoHandler.Create(gl);
        Vbo = VboHandler.Create(gl, false);
        Ibo = IboHandler.Create(gl, false);

        GlobalState.Renderer.ClearColor = SkyColors[(SkyState.Day, Plane.Fog)];
        var vertices = new Vertex[]
        {
            new(-0.5f, -0.5f, 0.0f, 0.0f, 0.0f),
            new(-0.5f, +0.5f, 0.0f, 0.0f, 1.0f),
            new(+0.5f, +0.5f, 0.0f, 1.0f, 1.0f),
            new(+0.5f, -0.5f, 0.0f, 1.0f, 0.0f),
        };
        var indices = new uint[] { 3, 0, 1, 3, 1, 2 };
        Vbo.Buffer<Vertex>(gl, vertices);
        Ibo.Buffer(gl, indices);
    }

    public static Vector4 Rgba(uint value)
    {
        var r = ((value & 0xFF000000) >> 24) / 255.0f;
        var g = ((value & 0x00FF0000) >> 16) / 255.0f;
        var b = ((value & 0x0000FF00) >> 08) / 255.0f;
        var a = ((value & 0x000000FF) >> 00) / 255.0f;
        return new(r, g, b, a);
    }

    public void Delete(GL gl)
    {
        Vao.Delete(gl);
        Vbo.Delete(gl);
        Ibo.Delete(gl);
    }

    private SkyState GetState()
    {
        var ticks = World.Ticks % CycleTicks;
        if (ticks <= HalfSunChangeTicks)
            return SkyState.SunRise;
        if (ticks <= DayTicks - HalfSunChangeTicks)
            return SkyState.Day;
        if (ticks <= DayTicks + HalfSunChangeTicks)
            return SkyState.SunSet;
        if (ticks <= CycleTicks - HalfSunChangeTicks)
            return SkyState.Night;
        return SkyState.SunRise;
    }

    private float GetStateProgress()
    {
        var ticks = (float)World.Ticks % CycleTicks;
        // if (ticks <= HalfSunChangeTicks)
        // {
        //     return 0.5f + (float)ticks / HalfSunChangeTicks;
        // }
        // if (ticks <= DayTicks - HalfSunChangeTicks)
        // {
        //     return (float)(ticks - HalfSunChangeTicks) / (DayTicks - SunChangeTicks);
        // }
        // if (ticks <= DayTicks + HalfSunChangeTicks)
        // {
        //     return (float)(ticks - (DayTicks - HalfSunChangeTicks)) / SunChangeTicks;
        // }
        // if (ticks <= CycleTicks - HalfSunChangeTicks)
        // {
        //     return (float)(ticks - (DayTicks + HalfSunChangeTicks)) / (NightTicks - SunChangeTicks);
        // }
        // return (float)(ticks - (CycleTicks - HalfSunChangeTicks)) / SunChangeTicks;
        //
        return State switch
        {
            SkyState.Day => (ticks - HalfSunChangeTicks) / DayTicks,
            SkyState.Night => (ticks - (DayTicks + HalfSunChangeTicks)) / NightTicks,
            SkyState.SunRise => ticks <= HalfSunChangeTicks
                ? 0.5f + ticks / HalfSunChangeTicks * 0.5f
                : (ticks - (CycleTicks - HalfSunChangeTicks)) / HalfSunChangeTicks * 0.5f,
            SkyState.SunSet => (ticks - (DayTicks - HalfSunChangeTicks)) / SunChangeTicks,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private SkyState GetDayNight()
    {
        var ticks = World.Ticks % CycleTicks;
        return (ticks >= 0 && ticks <= DayTicks) ? SkyState.Day : SkyState.Night;
    }

    private float GetDayNightProgress()
    {
        var ticks = World.Ticks % CycleTicks;
        return GetDayNight() switch
        {
            SkyState.Day => (float)ticks / DayTicks,
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
        return sca * rotZ * rotX * rotY * trans;
    }

    private Matrix4x4 CelestialModel(CelestialBody body, Vector3 center)
    {
        var isShow = StateDayNight == (body == CelestialBody.Sun ? SkyState.Day : SkyState.Night);
        const float baseStart = -(MathF.PI + 0.5f);
        const float baseEnd = 0.5f;
        (float Start, float End) show = (baseStart, baseEnd);
        (float Start, float End) hide = (
            baseEnd,
            MathF.IEEERemainder(MathF.Tau + baseStart, MathF.Tau)
        );

        var start = isShow ? show.Start : hide.Start;
        var end = isShow ? show.End : hide.End;
        var angle = MathF.IEEERemainder(
            MathF.Tau + DayNightProgress * (end - start) + start,
            MathF.Tau
        );

        var transY = Matrix4x4.CreateTranslation(Vector3.Add(center, new(0, 4, 0)));
        var rotY = Matrix4x4.CreateRotationY(float.DegreesToRadians(-90));
        var rotX = Matrix4x4.CreateRotationX(angle);
        var transZ = Matrix4x4.CreateTranslation(new(0, 0, 10));
        var sca = Matrix4x4.CreateScale(new Vector3(8, 8, 0));
        return sca * transZ * rotX * rotY * transY;
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
        Ibo.DrawElements(gl, GlobalState.Renderer.Wireframe);
    }

    private void RenderStars(GL gl, Vector3 center, Texture2D texture)
    {
        if (State is SkyState.Day)
            return;
        var a = State switch
        {
            SkyState.Night => 1f,
            SkyState.SunRise => MathHelper.LerpPrecise(1, 0, StateProgress),
            _ => MathHelper.LerpPrecise(0, 1, StateProgress),
        };
        var angle = (float)World.Ticks / CycleTicks * MathF.Tau;
        var random = new Random(0x57A125);
        for (var i = 0; i < 512; i++)
        {
            var angleOffset = float.DegreesToRadians(random.Next(0, 360));
            var scaleOffset = random.Next(-1, 6) * 0.05f;
            var offsetX = random.Next(-48, 48);
            var offsetY = random.Next(-48, 48);
            var offsetZ = random.Next(-48, 48);
            var offsetR = random.Next(-10, 0) / 100f;
            var offsetG = random.Next(-10, 0) / 100f;
            var offsetB = random.Next(-10, 0) / 100f;
            var twinkleRate = random.NextInt64(GlobalState.TickRate, GlobalState.TickRate * 2);
            var twinkle = MathHelper.RandomChance(random, 0.08)
                ? ((float)((World.Ticks + random.Next(0, 100)) % twinkleRate) / twinkleRate - 0.5f)
                    * 0.25f
                : 0;

            var transCenter = Matrix4x4.CreateTranslation(center);
            var rotY = Quaternion.CreateFromAxisAngle(new(0, 1, 0), float.DegreesToRadians(-90));
            var rotXyz = Quaternion.CreateFromAxisAngle(
                new(1.0f, -0.3f, 0.25f),
                angle + angleOffset
            );
            var transOffset = Matrix4x4.CreateTranslation(new(offsetX, offsetY, offsetZ));
            var sca = Matrix4x4.CreateScale(new Vector3(0.2f + scaleOffset, 0.2f + scaleOffset, 0));
            var model =
                sca
                * transOffset
                * Matrix4x4.CreateFromQuaternion(rotXyz)
                * Matrix4x4.CreateFromQuaternion(rotY)
                * transCenter;

            RenderPlane(
                gl,
                texture,
                new(1 + offsetR, 1 + offsetG, 1 + offsetB, a + twinkle),
                model
            );
        }
    }

    private void ColorsDayNight(
        SkyState state,
        out Vector4 sun,
        out Vector4 fog,
        out Vector4 sky,
        out Vector4 @void,
        out Vector4 cloud
    )
    {
        sun = SunlightColors[State];
        fog = SkyColors[(state, Plane.Fog)];
        sky = SkyColors[(state, Plane.Sky)];
        @void = SkyColors[(State, Plane.Void)];
        cloud = CloudColors[State];
    }

    private void ColorsTransition(
        SkyState state,
        SkyState from,
        SkyState to,
        out Vector4 sun,
        out Vector4 fog,
        out Vector4 sky,
        out Vector4 @void,
        out Vector4 cloud
    )
    {
        sun = Color.RgbaLerp(SunlightColors[from], SunlightColors[to], StateProgress);
        fog = Color.RgbaLerp3(
            SkyColors[(from, Plane.Fog)],
            SkyColors[(state, Plane.Fog)],
            SkyColors[(to, Plane.Fog)],
            StateProgress
        );
        sky = Color.RgbaLerp(
            SkyColors[(from, Plane.Sky)],
            SkyColors[(to, Plane.Sky)],
            StateProgress
        );
        @void = Color.RgbaLerp(
            SkyColors[(from, Plane.Void)],
            SkyColors[(to, Plane.Void)],
            StateProgress
        );
        cloud = Color.RgbaLerp(CloudColors[from], CloudColors[to], StateProgress);
    }

    public void Render(GL gl)
    {
        State = GetState();
        StateDayNight = GetDayNight();
        StateProgress = GetStateProgress();
        DayNightProgress = GetDayNightProgress();

        var center = GlobalState.Renderer.PerspectiveCamera.Position;

        // disable depth writing so the sky is always overwritten by everything else
        gl.DepthMask(false);

        Vector4 sunColor,
            fogColor,
            skyColor,
            voidColor,
            cloudColor;

        switch (State)
        {
            case SkyState.Day:
            case SkyState.Night:
                ColorsDayNight(
                    State,
                    out sunColor,
                    out fogColor,
                    out skyColor,
                    out voidColor,
                    out cloudColor
                );
                break;
            case SkyState.SunRise:
                ColorsTransition(
                    State,
                    SkyState.Night,
                    SkyState.Day,
                    out sunColor,
                    out fogColor,
                    out skyColor,
                    out voidColor,
                    out cloudColor
                );
                break;
            case SkyState.SunSet:
            default:
                ColorsTransition(
                    State,
                    SkyState.Day,
                    SkyState.Night,
                    out sunColor,
                    out fogColor,
                    out skyColor,
                    out voidColor,
                    out cloudColor
                );
                break;
        }
        SunlgihtColor = sunColor;
        FogColor = fogColor;
        ClearColor = fogColor;

        GlobalState.Renderer.UseShader(gl, Renderer.ShaderType.Sky);
        GlobalState.Renderer.SetViewProject(gl);
        var shader = GlobalState.Renderer.Shaders[Renderer.ShaderType.Sky];
        shader.UniformVector2(gl, "uv_offset", Vector2.Zero);
        shader.UniformVector4(gl, "fog_color", fogColor);
        shader.UniformFloat(gl, "fog_near", FogNear / 8.0f);
        shader.UniformFloat(gl, "fog_far", FogFar);

        // sky plane
        var model = PlaneModel(
            Vector3.Add(center, new(0, 16, 0)),
            new(float.DegreesToRadians(-90), 0, 0),
            new(1024.0f, 1024.0f, 0)
        );
        RenderPlane(gl, null, skyColor, model);

        // void plane
        model = PlaneModel(
            Vector3.Add(center, new(0, -16, 0)),
            new(float.DegreesToRadians(90), 0, 0),
            new(1024.0f, 1024.0f, 0)
        );
        RenderPlane(gl, null, voidColor, model);

        // stars
        RenderStars(gl, center, GlobalState.Renderer.Textures[Renderer.TextureType.Star]);

        // sun
        model = CelestialModel(CelestialBody.Sun, center);
        RenderPlane(
            gl,
            GlobalState.Renderer.Textures[Renderer.TextureType.Sun],
            Vector4.One,
            model
        );

        // moon
        model = CelestialModel(CelestialBody.Moon, center);
        RenderPlane(
            gl,
            GlobalState.Renderer.Textures[Renderer.TextureType.Moon],
            Vector4.One,
            model
        );

        gl.DepthMask(true);

        const float cloudsSize = 2048f;
        const float cloudsHeight = 192f;
        gl.Disable(EnableCap.CullFace);
        var pos = new Vector2(
            MathF.IEEERemainder(center.X, cloudsSize) / cloudsSize,
            (MathF.IEEERemainder(-center.Z, cloudsSize) / cloudsSize)
                + (World.Ticks % (CycleTicks / 3f)) / (CycleTicks / 3f)
        );
        shader.UniformVector2(gl, "uv_offset", pos);
        shader.UniformFloat(gl, "fog_near", FogNear * 5);
        shader.UniformFloat(gl, "fog_far", FogFar * 5);
        model = PlaneModel(
            new(center.X, cloudsHeight, center.Z),
            new(float.DegreesToRadians(-90), 0, 0),
            new(cloudsSize, cloudsSize, 0)
        );
        RenderPlane(
            gl,
            GlobalState.Renderer.Textures[Renderer.TextureType.Clouds],
            cloudColor,
            model
        );
        gl.Enable(GLEnum.CullFace);
    }
}
