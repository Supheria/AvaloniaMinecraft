using System;
using System.Collections.Generic;
using System.Numerics;
using AvaMc.WorldBuilds;

namespace AvaMc.Comparers;

public sealed class FaceDepthComparer : Comparer<Face>
{
    DepthOrder Order { get; }

    public FaceDepthComparer(DepthOrder order)
    {
        Order = order;
    }

    public override int Compare(Face? f1, Face? f2)
    {
        if (f1 is null || f2 is null)
            throw new ArgumentNullException();
        var sign = Math.Sign(f1.DistanceSquared - f2.DistanceSquared);
        return Order switch
        {
            DepthOrder.Nearer => sign,
            DepthOrder.Farther => -sign,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
