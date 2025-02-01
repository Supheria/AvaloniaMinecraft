using System;
using AvaMc.Util;
using AvaMc.WorldBuilds;
using Silk.NET.Maths;

namespace AvaMc.Blocks;

// TODO: not complete yet
public abstract partial class Block
{
    protected record Data
    {
        public BlockId Id { get; set; } = BlockId.Air;
        public bool Transparent { get; set; } = true;
    }

    public BlockId Id { get; }
    public bool Transparent { get; }

    protected Block(Data data)
    {
        Id = data.Id;
        Transparent = data.Transparent;
    }
    // public bool Liquid { get; set; }
    // public bool CanEmitLight { get; set; }
    // public bool Animated { get; set; }
    // public BlockMesh.MeshType MeshType { get; set; }
    // public bool Solid { get; set; }
    //
    // // for non-solid blocks
    // public float GravityModifier { get; set; }
    //
    // // for non-solid blocks
    // public float Drag { get; set; }
    //
    // // for solid blocks
    // public float Sliperiness { get; set; }
    //
    // // TODO: put somewhere else or not
    // public readonly record struct MeshInfo(
    //     Vector3 Offset,
    //     Vector3 Size,
    //     Vector2 UvOffset,
    //     Vector2 UvSize
    // );
    //
    public abstract Vector2D<int> GetTextureLocation(Direction direction) ;
    // public abstract Func<World, Vector3, Direction, MeshInfo> GetMeshInfo { get; set; }
    // public abstract Func<Vector2[]> GetAnimationFrameOffsets { get; set; }
    // public abstract Func<World, Vector3, TorchLight> GetTorchLight { get; set; }
    // public abstract Func<World, Vector3, Aabb> GetAabb { get; set; }
}
