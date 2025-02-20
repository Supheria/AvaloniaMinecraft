using System;
using System.IO;
using Avalonia.Platform;

namespace AvaMc.Assets;

public sealed class AssetsRead
{
    public static Uri GenerateUri(string folderName, string fileName)
    {
        var path = $"avares://AvaMc/Assets/{folderName}/{fileName}";
        return new(path);
    }

    public static Stream ReadTexture(string textureName)
    {
        var uri = GenerateUri("textures", textureName + ".png");
        var stream = AssetLoader.Open(uri);
        return stream;
    }

    public static string ReadVertex(string shaderName)
    {
        var uri = GenerateUri("shaders", shaderName + ".vert");
        return ReadToString(uri);
    }

    public static string ReadFragment(string shaderName)
    {
        var uri = GenerateUri("shaders", shaderName + ".frag");
        return ReadToString(uri);
    }

    private static string ReadToString(Uri uri)
    {
        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
