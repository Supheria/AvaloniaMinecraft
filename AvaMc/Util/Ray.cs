using System;
using System.Numerics;

namespace AvaMc.Util;

public readonly struct Ray
{
    public Vector3 Origin { get; }
    public Vector3 Direction { get; }

    public Ray(Vector3 origin, Vector3 direction)
    {
        Origin = origin;
        Direction = direction;
    }

    // TODO: ray block
    // public bool RayBlock(float maxDistance, out Vector3I blockPos, out Direction direction)
    // {
    //     var p = new Vector3I(
    //         MathHelper.FloorI(Origin.X),
    //         MathHelper.FloorI(Origin.Y),
    //         MathHelper.FloorI(Origin.Z)
    //     );
    //     var step = new Vector3I(
    //         Math.Sign(Direction.X),
    //         Math.Sign(Direction.Y),
    //         Math.Sign(Direction.Z)
    //     );
    //     var tmax = 
    // }
}
