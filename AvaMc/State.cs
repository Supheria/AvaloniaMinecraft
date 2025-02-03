using AvaMc.Gfx;
using AvaMc.WorldBuilds;

namespace AvaMc;

public sealed class State
{
    public const int TickRate = 60;
    public static int Ticks { get; set; }
    public static Game Game { get; set; } = new();
    public static BlockAtlas BlockAtlas { get; set; }
    public static ShaderHandler Shader { get; set; }
    public static World World { get; set; }
    public static bool Wireframe { get; set; }
    // public static Camera TestCamera { get; set; }
}
