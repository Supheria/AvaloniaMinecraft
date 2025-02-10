using System.Numerics;
using AvaMc.Gfx;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Blocks;

// TODO: not complete yet
public abstract partial class Block
{
    public abstract BlockId Id { get; }
    public virtual bool Transparent { get; } = false;
    public virtual bool Animated { get; } = false;
    public virtual bool Liquid { get; } = false;

    public virtual bool CanEmitLight { get; } = false;

    // public bool Animated { get; set; }
    public virtual BlockMeshType MeshType { get; } = BlockMeshType.Default;

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
    public abstract Vector2I GetTextureLocation(Direction direction);

    public virtual Vector2I[] GetAnimationFrameOffsets()
    {
        return [];
    }

    public virtual TorchLight GetTorchLight()
    {
        return TorchLight.Zero;
    }
    // public abstract Func<World, Vector3, Direction, MeshInfo> GetMeshInfo { get; set; }
    // public abstract Func<Vector2[]> GetAnimationFrameOffsets { get; set; }
    // public abstract Func<World, Vector3, TorchLight> GetTorchLight { get; set; }
    // public abstract Func<World, Vector3, Aabb> GetAabb { get; set; }
}
