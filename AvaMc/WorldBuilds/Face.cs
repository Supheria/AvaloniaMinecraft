using Microsoft.Xna.Framework;

namespace AvaMc.WorldBuilds;

public struct Face
{
    uint IndicesBase { get; set; }
    Vector3 Position { get; set; }
    float Distance { get; set; }
}
