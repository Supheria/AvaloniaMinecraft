using System.Collections.Generic;
using System.Linq;
using AvaMc.WorldBuilds;
using Hexa.NET.Utilities;

namespace AvaMc.Blocks;

public class BlockCollection
{
    public static Block GetBlock(BlockId id)
    {
        // TODO: temp
        return Blocks[id];
    }

    public static IEnumerable<Block> AllBlocks()
    {
        return Blocks.Values;
    }

    static Dictionary<BlockId, BlockGen> BlockGens { get; } =
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
            [BlockId.Torch] = new Torch(),
        };

    static UnsafeDictionary<BlockId, Block> Blocks { get; } =
        new()
        {
            [BlockId.Air] = BlockGens[BlockId.Air].Get(),
            [BlockId.Grass] = BlockGens[BlockId.Grass].Get(),
            [BlockId.Dirt] = BlockGens[BlockId.Dirt].Get(),
            [BlockId.Sand] = BlockGens[BlockId.Sand].Get(),
            [BlockId.Stone] = BlockGens[BlockId.Stone].Get(),
            [BlockId.Water] = BlockGens[BlockId.Water].Get(),
            [BlockId.Glass] = BlockGens[BlockId.Glass].Get(),
            [BlockId.Log] = BlockGens[BlockId.Log].Get(),
            [BlockId.Leaves] = BlockGens[BlockId.Leaves].Get(),
            [BlockId.Rose] = BlockGens[BlockId.Rose].Get(),
            [BlockId.Buttercup] = BlockGens[BlockId.Buttercup].Get(),
            [BlockId.Coal] = BlockGens[BlockId.Coal].Get(),
            [BlockId.Copper] = BlockGens[BlockId.Copper].Get(),
            [BlockId.Lava] = BlockGens[BlockId.Lava].Get(),
            [BlockId.Clay] = BlockGens[BlockId.Clay].Get(),
            [BlockId.Gravel] = BlockGens[BlockId.Gravel].Get(),
            [BlockId.Planks] = BlockGens[BlockId.Planks].Get(),
            [BlockId.Torch] = BlockGens[BlockId.Torch].Get(),
        };
}
