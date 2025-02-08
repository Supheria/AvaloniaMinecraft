using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Extensions;
using AvaMc.WorldBuilds;

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

    // finds the smallest possible t such that s + t * ds is an integer
    static Vector3 IntBound(Vector3 s, Vector3 ds)
    {
        var v = new Vector3();
        for (var i = 0; i < 3; i++)
        {
            v[i] =
                (ds[i] > 0 ? (MathF.Ceiling(s[i]) - s[i]) : (s[i] - MathF.Floor(s[i])))
                / MathF.Abs(ds[i]);
        }
        return v;
    }

    public bool RayBlock(float maxDistance, out BlockWorldPosition blockPos, out Direction? direction)
    {
        blockPos = BlockWorldPosition.Zero;
        direction = null;

        var p = new BlockWorldPosition(
            MathHelper.FloorI(Origin.X),
            MathHelper.FloorI(Origin.Y),
            MathHelper.FloorI(Origin.Z)
        );
        var step = new Vector3I(
            Math.Sign(Direction.X),
            Math.Sign(Direction.Y),
            Math.Sign(Direction.Z)
        );
        var tMax = IntBound(Origin, Direction);
        var tDelta = Vector3.Divide(step.ToNumerics(), Direction);
        var radius = maxDistance / Direction.Length();

        while (true)
        {
            if (GetBlockNotAir(p))
            {
                blockPos = p;
                return true;
            }

            if (tMax.X < tMax.Y)
            {
                if (tMax.X < tMax.Z)
                {
                    if (tMax.X > radius)
                        break;
                    p.X += step.X;
                    tMax.X += tDelta.X;
                    direction = Util.Direction.ToDirection(new(-step.X, 0, 0));
                }
                else
                {
                    if (tMax.Z > radius)
                        break;
                    p.Z += step.Z;
                    tMax.Z += tDelta.Z;
                    direction = Util.Direction.ToDirection(new(0, 0, -step.Z));
                }
            }
            else
            {
                if (tMax.Y < tMax.Z)
                {
                    if (tMax.Y > radius)
                        break;
                    p.Y += step.Y;
                    tMax.Y += tDelta.Y;
                    direction = Util.Direction.ToDirection(new(0, -step.Y, 0));
                }
                else
                {
                    if (tMax.Z > radius)
                        break;
                    p.Z += step.Z;
                    tMax.Z += tDelta.Z;
                    direction = Util.Direction.ToDirection(new(0, 0, -step.Z));
                }
            }
        }
        return false;
    }

    private static bool GetBlockNotAir(BlockWorldPosition position)
    {
        var id = GlobalState.World.GetBlockId(position);
        return id is not BlockId.Air;
    }
}
