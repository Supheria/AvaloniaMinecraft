using Avalonia;

namespace AvaMc;

public sealed class State
{
    public const int TickRate = 60;
    public static int Ticks { get; set; }
    public static Size WindowSize { get; set; }
}
