
using System.Numerics;
using Silk.NET.Maths;

namespace AvaMc.Util;

public sealed class OrthographicCamera : Camera
{
    public Vector2 Position { get; set; }
    public Vector2 Min { get; set; }
    public Vector2 Max { get; set; }
    
    public void Initialize(Vector2 min, Vector2 max)
    {
        Min = min;
        Max = max;
        Update();
    }
    
    public void Update()
    {
        var position = Position;
        var min = Min;
        var max = Max;
        
        View = Matrix4x4.Identity;
        // View = Matrix4x4.CreateTranslation(-position.X, -position.Y, 0f);
        Project = Matrix4x4.CreateOrthographicOffCenter(min.X, max.X, min.Y, max.Y, -10f, 10f);
    }
}