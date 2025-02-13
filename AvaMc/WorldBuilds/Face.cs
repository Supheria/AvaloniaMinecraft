using System.Numerics;
using AvaMc.Coordinates;

namespace AvaMc.WorldBuilds;

public struct Face
{
    public int IndicesBase { get; set; }
    public Vector3 Position { get; }

    public Face(int indicesBase, Vector3 position)
    {
        IndicesBase = indicesBase;
        Position = position;
    }
}
