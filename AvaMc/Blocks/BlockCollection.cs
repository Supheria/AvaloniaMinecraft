using System.Collections.Generic;
using System.Linq;
using AvaMc.WorldBuilds;
using Hexa.NET.Utilities;

namespace AvaMc.Blocks;

public class BlockCollection
{
    public static unsafe Block* GetBlock(BlockId id)
    {
        return Blocks.GetPointer((int)id);
    }

    public static IEnumerable<Block> AllBlocks()
    {
        return Blocks;
    }

    static UnsafeList<Block> Blocks { get; } =
        [
            Air.Get(),
            Grass.Get(),
            Dirt.Get(),
            Sand.Get(),
            Stone.Get(),
            Water.Get(),
            Glass.Get(),
            Log.Get(),
            Leaves.Get(),
            Rose.Get(),
            Buttercup.Get(),
            Coal.Get(),
            Copper.Get(),
            Lava.Get(),
            Clay.Get(),
            Gravel.Get(),
            Planks.Get(),
            Torch.Get(),
        ];
}
