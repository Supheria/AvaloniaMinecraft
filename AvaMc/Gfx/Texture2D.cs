using System;
using System.IO;
using System.Runtime.InteropServices;
using AvaMc.Assets;
using AvaMc.Util;
using Silk.NET.OpenGLES;
using StbImageSharp;

namespace AvaMc.Gfx;

public readonly struct Texture2D
{
    uint Handle { get; }
    public int Plot { get; }
    public Vector2I Size { get; }

    public record struct ImageInfo(byte[] Pixels, int Width, int Height, int ColumnNumber);

    private Texture2D(uint handle, int plot, int width, int height)
    {
        Handle = handle;
        Plot = plot;
        Size = new(width, height);
    }

    public static Texture2D Create(GL gl, string textureName, int plot)
    {
        using var stream = AssetsRead.ReadTexture(textureName);
        return Create(gl, stream, plot);
    }

    public static Texture2D Create(GL gl, Stream stream, int plot)
    {
        var image = LoadImage(stream);
        return Create(gl, plot, image.Pixels, image.Width, image.Height, image.ColumnNumber);
    }

    public static Texture2D Create(
        GL gl,
        int plot,
        byte[] pixels,
        int width,
        int height,
        int columnNumber
    )
    {
        var handle = GetHandle(gl, plot, pixels, width, height, columnNumber);
        return new(handle, plot, width, height);
    }

    public static unsafe ImageInfo LoadImage(Stream stream)
    {
        var ptr = (void*)null;
        try
        {
            int width;
            int height;
            int column;
            StbImage.stbi_set_flip_vertically_on_load(1);
            ptr = StbImage.stbi__load_and_postprocess_8bit(
                new StbImage.stbi__context(stream),
                &width,
                &height,
                &column,
                0
            );
            var data = new byte[width * height * column];
            Marshal.Copy(new IntPtr(ptr), data, 0, data.Length);
            return new ImageInfo(data, width, height, column);
        }
        finally
        {
            if ((IntPtr)ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(new IntPtr(ptr));
        }
    }

    private static uint GetHandle(
        GL gl,
        int plot,
        byte[] pixels,
        int width,
        int height,
        int columnNumber
    )
    {
        // ImageResult.FromStream()
        gl.ActiveTexture(TextureUnit.Texture0 + plot);

        var handle = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, handle);

        gl.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.Repeat
        );
        gl.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.Repeat
        );
        gl.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest
        );
        gl.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMinFilter.Nearest
        );

        InternalFormat internalFormat;
        PixelFormat pixelFormat;
        switch (columnNumber)
        {
            case 4:
                internalFormat = InternalFormat.Rgba;
                pixelFormat = PixelFormat.Rgba;
                break;
            case 3:
                internalFormat = InternalFormat.Rgb;
                pixelFormat = PixelFormat.Rgb;
                break;
            case 1:
                internalFormat = InternalFormat.Red;
                pixelFormat = PixelFormat.Red;
                break;
            default:
                throw new ArgumentException("Automatic Texture type recognition failed");
        }
        gl.TexImage2D<byte>(
            TextureTarget.Texture2D,
            0,
            internalFormat,
            (uint)width,
            (uint)height,
            0,
            pixelFormat,
            PixelType.UnsignedByte,
            pixels
        );

        gl.BindTexture(TextureTarget.Texture2D, 0);

        return handle;
    }

    public void Bind(GL gl)
    {
        gl.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void Unbind(GL gl)
    {
        gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Delete(GL gl)
    {
        gl.DeleteTexture(Handle);
    }
}
