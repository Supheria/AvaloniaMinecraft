using AvaMc.Blocks;
using AvaMc.WorldBuilds;

namespace AvaMc.Extensions;

public static class BlockDataExtensions
{
    public static Block Block(this BlockData data)
    {
        return Blocks.Block.Get(data);
    }
}