using System.Collections.Concurrent;
using System.Numerics;
using Avalonia.Input;

namespace AvaMc.Gfx;

public sealed class Pointer
{
    ConcurrentDictionary<PointerButton, Button> Buttons { get; } = [];
    public Vector2 Position { get; set; }
    public Vector2 Delta { get; set; }

    public Button this[PointerButton pointer]
    {
        get
        {
            if (Buttons.TryGetValue(pointer, out var button))
                return button;
            Buttons[pointer] = new();
            return Buttons[pointer];
        }
    }
    
    public void Tick()
    {
        foreach (var button in Buttons.Values)
        {
            button.PressedTick = button.Down && !button.LastTick;
            button.LastTick = button.Down;
        }
    }
    
    public void Update()
    {
        foreach (var button in Buttons.Values)
        {
            button.Pressed = button.Down && !button.Last;
            button.Last = button.Down;
        }
    }
}
