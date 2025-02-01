using System.Numerics;
using Avalonia;

namespace AvaMc.Gfx;

public sealed class Game
{
    public Size WindowSize { get; set; }
    public Keyboard Keyboard {get;} = new();
    public Pointer Pointer {get;} = new();
    public long FrameDelta { get; set; }
}