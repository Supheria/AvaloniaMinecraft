using AvaMc.Gfx;
using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Blocks;

public sealed class Torch : Block
{
    public override BlockId Id { get; } = BlockId.Torch;
    public override bool Transparent { get; } = true;
    public override bool CanEmitLight { get; } = true;
    public override BlockMeshType MeshType { get; } = BlockMeshType.Torch;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(0, 2);
    }

    public override TorchLight GetTorchLight()
    {
        return new(15, 15, 15, 15);
    }
}
