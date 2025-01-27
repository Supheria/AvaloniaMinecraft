using System;
using AvaMc.Util;
using AvaMc.WorldBuilds;
using Microsoft.Xna.Framework;

namespace AvaMc.Blocks;

// TODO: not complete yet
public abstract class Block
{
    public const BlockId BlockIdLast = BlockId.PineLeaves;
    public BlockId Id { get; set; }
    public bool Transparent { get; set; }
    public bool Liquid { get; set; }
    public bool CanEmitLight { get; set; }
    public bool Animated { get; set; }
    public BlockMesh.MeshType MeshType { get; set; }
    public bool Solid { get; set; }

    // for non-solid blocks
    public float GravityModifier { get; set; }

    // for non-solid blocks
    public float Drag { get; set; }

    // for solid blocks
    public float Sliperiness { get; set; }

    // TODO: put somewhere else or not
    public readonly record struct MeshInfo(
        Vector3 Offset,
        Vector3 Size,
        Vector2 UvOffset,
        Vector2 UvSize
    );

    public abstract Func<World, Vector3, Direction, Vector2> GetTextureLocation { get; set; }
    public abstract Func<World, Vector3, Direction, MeshInfo> GetMeshInfo { get; set; }
    public abstract Func<Vector2[]> GetAnimationFrames { get; set; }
    public abstract Func<World, Vector3, TorchLight> GetTorchLight { get; set; }
    public abstract Func<World, Vector3, Aabb> GetAabb { get; set; }
}
