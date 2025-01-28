using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AvaMc.Assets;
using AvaMc.Extensions;
using AvaMc.Util;
using Microsoft.Xna.Framework;
using Silk.NET.OpenGLES;

namespace AvaMc.Gfx;

// TODO: not test yet
public sealed class Renderer
{
    public enum FillMode : byte
    {
        Fill,
        Line,
    }

    public enum ShaderType : byte
    {
        None,
        Chunk,
        Sky,
        BasicTexture,
        BasicColor,
    }

    public enum TextureType : byte
    {
        CrossHair,
        Clouds,
        Star,
        Sun,
        Moon,
        Hotbar,
    }

    public enum RenderPass : byte
    {
        Pass2D,
        Pass3D,
    }

    public const int CameraStackMax = 256;
    public CameraType CurrentCameraType { get; private set; } = CameraType.Perspective;
    public ConcurrentStack<CameraType> CameraStack { get; } = [];
    public PerspectiveCamera PerspectiveCamera { get; } = new();
    public OrthographicCamera OrthographicCamera { get; } = new();
    public Dictionary<ShaderType, Shader> Shaders { get; }
    public ShaderType CurrentShaderType { get; set; } = ShaderType.None;
    public Shader Shader { get; set; } = new();
    public Dictionary<TextureType, Texture2D> Textures { get; }
    public BlockAtlas BlockAtlas { get; private set; }
    public Vector4 ClearColor { get; set; }
    public VaoHandler Vao { get; }
    public VboHandler Vbo { get; }
    public IboHandler Ibo { get; }
    public bool Wireframe { get; set; }

    public Renderer(GL gl)
    {
        Shaders = InitializeShaders(gl);
        Textures = InitializeTextures(gl);
        BlockAtlas = BlockAtlas.Create(gl, "blocks", new(16, 16));
        Vao = VaoHandler.Create(gl);
        Vbo = VboHandler.Create(gl, true);
        Ibo = IboHandler.Create(gl, true);
        PerspectiveCamera.Initialize(75);
        OrthographicCamera.Initialize(Vector2.Zero, State.WindowSize.ToVector2());
    }

    private static Dictionary<ShaderType, Shader> InitializeShaders(GL gl)
    {
        // csharpier-ignore
        return new()
        {
            [ShaderType.BasicTexture] = Shader.Create(gl, "basic_texture", new()
            {
                [0] = "position",
                [1] = "uv",
            }),
            [ShaderType.Chunk] = Shader.Create(gl, "chunk", new()
            {
                [0] = "position",
                [1] = "uv",
                [2] = "color",
            }),
            [ShaderType.Sky] = Shader.Create(gl, "sky", new()
            {
                [0] = "position",
                [1] = "uv",
            }),
            [ShaderType.BasicColor] = Shader.Create(gl, "basic_color", new()
            {
                [0] = "position",
            }),
        };
    }

    private static Dictionary<TextureType, Texture2D> InitializeTextures(GL gl)
    {
        return new()
        {
            [TextureType.CrossHair] = Texture2D.Create(gl, "crosshair", 0),
            [TextureType.Clouds] = Texture2D.Create(gl, "clouds", 0),
            [TextureType.Star] = Texture2D.Create(gl, "star", 0),
            [TextureType.Sun] = Texture2D.Create(gl, "sun", 0),
            [TextureType.Moon] = Texture2D.Create(gl, "moon", 0),
            [TextureType.Hotbar] = Texture2D.Create(gl, "hotbar", 0),
        };
    }

    public void Delete(GL gl)
    {
        foreach (var shader in Shaders.Values)
            shader.Delete(gl);
        foreach (var texture in Textures.Values)
            texture.Delete(gl);
        BlockAtlas.Delete(gl);
        Vao.Delete(gl);
        Vbo.Delete(gl);
        Ibo.Delete(gl);
    }

    public void Prepare(GL gl, RenderPass pass)
    {
        if (pass is RenderPass.Pass2D)
        {
            OrthographicCamera.Initialize(Vector2.Zero, State.WindowSize.ToVector2());
            gl.Clear(ClearBufferMask.DepthBufferBit);
            // gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            gl.Disable(EnableCap.DepthTest);
            gl.Disable(EnableCap.CullFace);
            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
        else if (pass is RenderPass.Pass3D)
        {
            gl.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // gl.PolygonMode(
            //     TriangleFace.FrontAndBack,
            //     Wireframe ? PolygonMode.Line : PolygonMode.Fill
            // );
            gl.Enable(EnableCap.DepthTest);
            gl.DepthFunc(DepthFunction.Less);
            gl.Enable(EnableCap.CullFace);
            gl.CullFace(TriangleFace.Back);
            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
    }

    public void SetCamera(CameraType type)
    {
        CurrentCameraType = type;
    }

    public void PushCamera()
    {
        if (CameraStack.Count > CameraStackMax)
            throw new ArgumentOutOfRangeException($"camera stack count out range");
        CameraStack.Push(CurrentCameraType);
    }

    public void PopCamera()
    {
        if (CameraStack.TryPop(out var type))
            CurrentCameraType = type;
    }

    public void SetViewProject(GL gl)
    {
        var camera = CurrentCameraType switch
        {
            CameraType.Perspective => PerspectiveCamera.GetViewProject(),
            CameraType.Orthographic => OrthographicCamera.GetViewProject(),
            _ => throw new ArgumentOutOfRangeException(),
        };
        Shader.UniformViewProject(gl, camera);
    }

    public void UseShader(GL gl, ShaderType type)
    {
        if (type == CurrentShaderType)
            return;
        CurrentShaderType = type;
        Shader = Shaders[type];
        Shader.Use(gl);
    }

    public void RenderQuadColor(GL gl, Vector2 size, Vector4 color, Matrix4 model)
    {
        UseShader(gl, ShaderType.BasicColor);
        SetViewProject(gl);
        Shader.UniformMatrix4(gl, "m", model);
        Shader.UniformVector4(gl, "color", color);
        // csharpier-ignore
        Vbo.Buffer(gl, [
            0, 0, 0,
            0, size.Y, 0,
            size.X, size.Y, 0,
            size.X, 0, 0
        ]);
        Ibo.Buffer(gl, [3, 0, 1, 3, 1, 2]);
        Vao.Link(gl, Vbo, 0, 3, VertexAttribPointerType.Float, 0);

        Vao.Bind(gl);
        Ibo.DrawElements(gl, Wireframe);
    }

    public void RenderQuadTexture(
        GL gl,
        Texture2D texture,
        Vector2 size,
        Vector4 color,
        Vector2 uvMin,
        Vector2 uvMax,
        Matrix4 model
    )
    {
        UseShader(gl, ShaderType.BasicTexture);
        SetViewProject(gl);
        Shader.UniformMatrix4(gl, "m", model);
        Shader.UniformTexture(gl, "tex", texture);
        Shader.UniformVector4(gl, "color", color);
        // csharpier-ignore
        Vbo.Buffer(gl, [
            0, 0, 0,
            0, size.Y, 0,
            size.X, size.Y, 0,
            size.X, 0, 0,

            uvMin.X, uvMin.Y,
            uvMin.X, uvMax.Y,
            uvMax.X, uvMax.Y,
            uvMax.X, uvMin.Y
        ]);
        Ibo.Buffer(gl, [3, 0, 1, 3, 1, 2]);
        Vao.Link(gl, Vbo, 0, 3, VertexAttribPointerType.Float, 0);
        Vao.Link(gl, Vbo, 1, 2, VertexAttribPointerType.Float, (3 * 4) * sizeof(float));

        Vao.Bind(gl);
        Ibo.DrawElements(gl, Wireframe);
    }

    public void RenderAabb(GL gl, Aabb aabb, Vector4 color, Matrix4 model, FillMode fillMode)
    {
        UseShader(gl, ShaderType.BasicColor);
        SetViewProject(gl);
        Shader.UniformMatrix4(gl, "m", model);
        Shader.UniformVector4(gl, "color", color);
        // csharpier-ignore
        uint[] indices =
        [
            1, 0, 3, 1, 3, 2, // north (-z)
            4, 5, 6, 4, 6, 7, // south (+z)
            5, 1, 2, 5, 2, 6, // east (+x)
            0, 4, 7, 0, 7, 3, // west (-x)
            2, 3, 7, 2, 7, 6, // top (+y)
            5, 4, 0, 5, 0, 1  // bottom (-y)
        ];

        var min = aabb.Min;
        var max = aabb.Max;
        // csharpier-ignore
        float[] vertices = {
            min.X, min.Y, min.Z,
            max.X, min.Y, min.Z,
            max.X, max.Y, min.Z,
            min.X, max.Y, min.Z,

            min.X, min.Y, max.Z,
            max.X, min.Y, max.Z,
            max.X, max.Y, max.Z,
            min.X, max.Y, max.Z,
        };
        Vbo.Buffer(gl, vertices);
        Ibo.Buffer(gl, indices);
        Vao.Link(gl, Vbo, 0, 3, VertexAttribPointerType.Float, 0);

        Vao.Bind(gl);
        Ibo.DrawElements(gl, fillMode is FillMode.Line || Wireframe);
    }
}
