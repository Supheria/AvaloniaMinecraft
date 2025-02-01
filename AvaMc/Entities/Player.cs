using AvaMc.Util;
using AvaMc.WorldBuilds;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace AvaMc.Entities;

// TODO
public class Player
{
    const float MouseSensitivity = 3.2f;
    World World { get; set; }
    public PerspectiveCamera Camera { get; set; }
    bool HasLookBlock { get; set; }
    Vector3D<int> LookBlock { get; set; }
    Direction LookFace { get; set; }
    
    public Player(World world)
    {
        World = world;
        Camera = new();
        Camera.Initialize(float.DegreesToRadians(75));
    }
    
    public void Delete(GL gl)
    {
        
    }
    
    public void Render(GL gl)
    {
        
    }
    
    public void Update()
    {
        Camera.Update();
        
    }
}
