using System;
using System.Collections.Generic;
using AvaMc.Coordinates;
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
        var d1 = Vector3I.DistanceSquared(Center, v1);
        var d2 = Vector3I.DistanceSquared(Center, v2);
        var sign = Math.Sign(d1 - d2);
        return Order switch
        {
            DepthOrder.Nearer => sign,
            DepthOrder.Farther => -sign,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
