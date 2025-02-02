using System;
using System.Collections.Generic;
using System.Numerics;

namespace AvaMc.Util;

public sealed class DistanceToCenterComparerF : Comparer<Vector3>
{
    public enum Order
    {
        Nearer,
        Farther,
    }

    Vector3 Center { get; }

    Order CompareOrder { get; }

    public DistanceToCenterComparerF(Vector3 center, Order order)
    {
        Center = center;
        CompareOrder = order;
    }

    public override int Compare(Vector3 f1, Vector3 f2)
    {
        var l1 = Vector3.Subtract(Center, f1).LengthSquared();
        var l2 = Vector3.Subtract(Center, f2).LengthSquared();
        var sign = Math.Sign(l1 - l2);
        return CompareOrder switch
        {
            Order.Nearer => sign,
            Order.Farther => -sign,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
