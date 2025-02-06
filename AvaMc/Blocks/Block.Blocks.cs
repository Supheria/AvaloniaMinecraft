using System.Collections.Generic;
using System.Linq;
using AvaMc.WorldBuilds;

namespace AvaMc.Blocks;

partial class Block
{
    public static Block Get(BlockId id)
    {
        return Blocks[id];
    }
    
    public static Block[] AllBlocks()
    {
        return Blocks.Values.ToArray();
    }
    
    static Dictionary<BlockId, Block> Blocks { get; } =
        new()
        {
            [BlockId.Air] = new Air(),
            [BlockId.Grass] = new Grass(),
            [BlockId.Dirt] = new Dirt(),
            [BlockId.Sand] = new Sand(),
            [BlockId.Stone] = new Stone(),
            [BlockId.Water] = new Water(),
            [BlockId.Glass] = new Glass(),
            [BlockId.Log] = new Log(),
            [BlockId.Leaves] = new Leaves(),
            [BlockId.Rose] = new Rose(),
            [BlockId.Buttercup] = new Buttercup(),
            [BlockId.Coal] = new Coal(),
            [BlockId.Copper] = new Copper(),
            [BlockId.Lava] = new Lava(),
            [BlockId.Clay] = new Clay(),
            [BlockId.Gravel] = new Gravel(),
            [BlockId.Planks] = new Planks(),
        };
}
