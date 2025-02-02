using System;
using System.Collections.Generic;
using AvaMc.Util;

namespace AvaMc.Comparers;

public sealed class ChunkDepthComparer : IComparer<Vector3I>
{
    Vector3I Center { get; }
    DepthOrder Order { get; }

    public ChunkDepthComparer(Vector3I center, DepthOrder order)
    {
        Center = center;
        Order = order;
    }

    public int Compare(Vector3I v1, Vector3I v2)
    {
        var l1 = Vector3I.Subtract(Center, v1).LengthSquared();
        var l2 = Vector3I.Subtract(Center, v2).LengthSquared();
        var sign = Math.Sign(l1 - l2);
        return Order switch
        {
            DepthOrder.Nearer => sign,
            DepthOrder.Farther => -sign,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
