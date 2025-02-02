using System.Collections.Concurrent;
using Avalonia.Input;

namespace AvaMc.Gfx;

public sealed class Keyboard
{
    ConcurrentDictionary<Key, Button> Keys { get; } = [];
    
    public Button this[Key key]
    {
        get
        {
            if (Keys.TryGetValue(key, out var button))
                return button;
            Keys[key] = new();
            return Keys[key];
        }
    }
    
    public void Tick()
    {
        foreach (var key in Keys.Values)
        {
            key.PressedTick = key.Down && !key.LastTick;
            key.LastTick = key.Down;
        }
    }
    
    public void Update()
    {
        foreach (var key in Keys.Values)
        {
            key.Pressed = key.Down && !key.Last;
            key.Last = key.Down;
        }
    }
}
