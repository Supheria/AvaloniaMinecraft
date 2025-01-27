using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Microsoft.Xna.Framework;
using Silk.NET.OpenGLES;
using StbImageSharp;

namespace AvaMc.Gfx;

public sealed unsafe class Texture2D : Resource
{
    public int Plot { get; }
    public Vector2 Size { get; }

    public record struct ImageInfo(byte[] Data, int Width, int Height, int ColumnNumber);
    
    private Texture2D(GL gl, ImageInfo image, int plot)
    {
        Plot = plot;
        Size = new(image.Width, image.Height);
        Handle = Load(gl, image, plot);
    }

    private static uint Load(GL gl, ImageInfo image, int unit)
    {
        // ImageResult.FromStream()
        gl.ActiveTexture(TextureUnit.Texture0 + unit);

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
        switch (image.ColumnNumber)
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
            (uint)image.Width,
            (uint)image.Height,
            0,
            pixelFormat,
            PixelType.UnsignedByte,
            image.Data
        );

        gl.BindTexture(TextureTarget.Texture2D, 0);

        return handle;
    }

    public static ImageInfo LoadImage(Stream stream)
    {
        var ptr = (void*)null;
        try
        {
            int width;
            int height;
            int column;
            // TODO: may remove flip
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
    
    public static Texture2D CreateFromStream(GL gl, Stream stream, int plot)
    {
        var image = LoadImage(stream);
        return new(gl, image, plot);
    }
    
    public static Texture2D CreateFromImage(GL gl, ImageInfo image, int plot)
    {
        return new(gl, image, plot);
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
