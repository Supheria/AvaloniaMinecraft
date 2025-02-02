using System;
using System.Collections.Generic;
using System.Numerics;
using AvaMc.WorldBuilds;

namespace AvaMc.Comparers;

public sealed class FaceDepthComparer : Comparer<Face>
{
    Vector3 Center { get; }
    DepthOrder Order { get; }

    public FaceDepthComparer(Vector3 center, DepthOrder order)
    {
        Center = center;
        Order = order;
    }

    public override int Compare(Face? f1, Face? f2)
    {
        if (f1 is null || f2 is null)
            throw new ArgumentNullException();
        var l1 = Vector3.Subtract(Center, f1.Position).LengthSquared();
        var l2 = Vector3.Subtract(Center, f2.Position).LengthSquared();
        var sign = Math.Sign(l1 - l2);
        return Order switch
        {
            DepthOrder.Nearer => sign,
            DepthOrder.Farther => -sign,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
