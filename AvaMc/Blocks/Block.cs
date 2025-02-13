using System;
using System.Numerics;
using AvaMc.Gfx;
using AvaMc.Util;
using AvaMc.WorldBuilds;
using Hexa.NET.Utilities;

namespace AvaMc.Blocks;

// TODO: not complete yet
public unsafe struct Block
{
    public BlockId Id { get; set; }
    public bool Transparent { get; set; }
    public bool Animated { get; set; }
    public bool Liquid { get; set; }
    public bool CanEmitLight { get; set; }
    public TorchLight TorchLight { get; set; }
    public BlockMeshType MeshType { get; set; }
    public UnsafeDictionary<Direction, Vector2I> TextureLocation { get; set; }
    public Vector2I* FrameOffsets { get; set; }

    public Block()
    {
        Id = BlockId.Air;
        Transparent = false;
        Animated = false;
        Liquid = false;
        CanEmitLight = false;
        TorchLight = TorchLight.Zero;
        MeshType = BlockMeshType.Default;
        TextureLocation = new()
        {
            [Direction.North] = Vector2I.Zero,
            [Direction.South] = Vector2I.Zero,
            [Direction.East] = Vector2I.Zero,
            [Direction.West] = Vector2I.Zero,
            [Direction.Up] = Vector2I.Zero,
            [Direction.Down] = Vector2I.Zero,
        };
        FrameOffsets = null;
    }

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
    // public Func<Direction, Vector2> GetTextureLocation { get; set; }
    // public Func<Vector2[]> GetAnimationFrameOffsets { get; set; }
    // public Func<TorchLight> GetTorchLight { get; set; }
    // public abstract Func<World, Vector3, Direction, MeshInfo> GetMeshInfo { get; set; }
    // public abstract Func<Vector2[]> GetAnimationFrameOffsets { get; set; }
    // public abstract Func<World, Vector3, TorchLight> GetTorchLight { get; set; }
    // public abstract Func<World, Vector3, Aabb> GetAabb { get; set; }
}
