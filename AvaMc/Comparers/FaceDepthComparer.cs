using System;
using System.Collections.Generic;
using System.Numerics;
using AvaMc.WorldBuilds;

namespace AvaMc.Comparers;

public sealed class FaceDepthComparer : IComparer<Face>
{
    Vector3 Center { get; }
    DepthOrder Order { get; }

    public FaceDepthComparer(Vector3 center, DepthOrder order)
    {
        Center = center;
        Order = order;
    }

    public int Compare(Face f1, Face f2)
    {
        var d1 = Vector3.DistanceSquared(Center, f1.Position);
        var d2 = Vector3.DistanceSquared(Center, f2.Position);
        return Order switch
        {
            DepthOrder.Nearer => Math.Sign(d1 - d2),
            DepthOrder.Farther => Math.Sign(d2 - d1),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
