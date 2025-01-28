using Microsoft.Xna.Framework;

namespace AvaMc.Util;

public abstract class Camera
{
    protected Matrix4 View { get; set; }
    protected Matrix4 Project { get; set; }
    
    public ViewProject GetViewProject()
    {
        return new(Project, View);
    }
}
