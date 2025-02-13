using System.Numerics;

namespace AvaMc.WorldBuilds;

partial class BlockMesh
{
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
    public static Vector3[] FaceCenters { get; } =
    [
        new(0.5f, 0.5f, 0.0f), // north (-z)
        new(0.5f, 0.5f, 1.0f), // south (+z)
        new(1.0f, 0.5f, 0.5f), // east (+x)
        new(0.0f, 0.5f, 0.5f), // west (-x)
        new(0.5f, 1.0f, 0.5f), // top (+y)
        new(0.5f, 0.0f, 0.5f), // bottom (-y)
    ];

    // csharpier-ignore
    static uint[] SpriteIndices { get; } =
    [
        3, 5, 0, 3, 6, 5,
        2, 1, 4, 2, 4, 7,
        3, 0, 5, 3, 5, 6,
        2, 4, 1, 2, 7, 4,
    ];

    // csharpier-ignore
    static float[] CubeVertices { get; } =
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
    static bool[] UvMaxOrNot { get; } =
    [
        true, false,
        false, false,
        false, true,
        true, true
    ];

    // TODO: may not good here
    private static float CubeUv(int index, float min, float max)
    {
        return UvMaxOrNot[index] ? max : min;
    }
}
