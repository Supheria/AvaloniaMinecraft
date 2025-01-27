using System.Collections.Concurrent;
using System.Collections.Generic;
using AvaMc.Util;

namespace AvaMc.Gfx;

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
        Texture,
        Color,
    }

    public const ShaderType ShaderLast = ShaderType.Color;

    public enum TextureType : byte
    {
        CrossHair,
        Clouds,
        Star,
        Sun,
        Moon,
        Hotbar,
    }

    public const TextureType TextureLast = TextureType.Hotbar;

    public enum RenderPass : byte
    {
        Pass2D,
        Pass3D,
    }

    public const int CameraStackMax = 256;
    public CameraType CameraType { get; set; }
    public ConcurrentStack<CameraType> CameraStack { get; } = new();
    public PerspectiveCamera PerspectiveCamera { get; }
    public OrthographicCamera OrthographicCamera { get; }
    public Dictionary<ShaderType, Shader> Shaders { get; } = [];
    public ShaderType CurrentShader { get; set; }
    public Shader Shader { get; set; }
    public Dictionary<TextureType, Texture2D> Textures { get; } = [];
}
