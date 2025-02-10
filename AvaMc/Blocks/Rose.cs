using AvaMc.Util;
using AvaMc.WorldBuilds;

namespace AvaMc.Blocks;

public sealed class Rose : Block
{
    public override BlockId Id { get; } = BlockId.Rose;
    public override bool Transparent { get; } = true;
    public override BlockMeshType MeshType { get; } = BlockMeshType.Sprite;

    public override Vector2I GetTextureLocation(Direction direction)
    {
        return new(0, 3);
    }
}
