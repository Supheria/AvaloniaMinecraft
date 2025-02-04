using AvaMc.Util;

namespace AvaMc.WorldBuilds;

public sealed class ChunkData
{
    public const int ChunkSizeX = 32;
    public const int ChunkSizeY = 32;
    public const int ChunkSizeZ = 32;
    public static Vector3I ChunkSize { get; } = new(ChunkSizeX, ChunkSizeY, ChunkSizeZ);
    public static uint[] FaceIndices { get; } = [1, 0, 3, 1, 3, 2];
    public static int[] UniqueIndices { get; } = [1, 0, 5, 2];

    // csharpier-ignore
    public static int[] CubeIndices { get; } =
        [
            1, 0, 3, 1, 3, 2, // north (-z)
            4, 5, 6, 4, 6, 7, // south (+z)
            5, 1, 2, 5, 2, 6, // east (+x)
            0, 4, 7, 0, 7, 3, // west (-x)
            2, 3, 7, 2, 7, 6, // top (+y)
            5, 4, 0, 5, 0, 1, // bottom (-y)
        ];

    // csharpier-ignore
    public static uint[] SpriteIndices { get; } =
        [
            3, 0, 5, 3, 5, 6,
            4, 2, 1, 4, 2, 7
        ];

    // csharpier-ignore
    public static float[] CubeVertices { get; } =
        [
            0, 0, 0,
            1, 0, 0,
            1, 1, 0,
            0, 1, 0,

            0, 0, 1,
            1, 0, 1,
            1, 1, 1,
            0, 1, 1
        ];

    // csharpier-ignore
    public static float[] CubeUvs { get; } =
        [
            1, 0,
            0, 0,
            0, 1,
            1, 1
        ];
}
