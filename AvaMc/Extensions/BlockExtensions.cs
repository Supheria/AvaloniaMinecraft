using AvaMc.Blocks;
using AvaMc.WorldBuilds;

namespace AvaMc.Extensions;

public static class BlockExtensions
{
    public static Block Block(this BlockId id)
    {
        return BlockCollection.GetBlock(id);
    }
}