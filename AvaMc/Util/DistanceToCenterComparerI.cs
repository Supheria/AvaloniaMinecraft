using System;
using System.Collections.Generic;

namespace AvaMc.Util;

public sealed class DistanceToCenterComparerI : Comparer<Vector3I>
{
    public enum Order
    {
        Nearer,
        Farther,
    }

    Vector3I Center { get; }

    Order CompareOrder { get; }

    public DistanceToCenterComparerI(Vector3I center, Order order)
    {
        Center = center;
        CompareOrder = order;
    }

    public override int Compare(Vector3I f1, Vector3I f2)
    {
        var l1 = Vector3I.Subtract(Center, f1).LengthSquared();
        var l2 = Vector3I.Subtract(Center, f2).LengthSquared();
        var sign = Math.Sign(l1 - l2);
        return CompareOrder switch
        {
            Order.Nearer => sign,
            Order.Farther => -sign,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
