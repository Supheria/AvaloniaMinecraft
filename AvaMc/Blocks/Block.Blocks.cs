using System.Collections.Generic;

namespace AvaMc.Blocks;

partial class Block
{
    // TODO
    public static readonly Dictionary<BlockId, Block> Blocks = new()
    {
        [BlockId.Air] = new Air(),
        [BlockId.Grass] = new Grass(),
        [BlockId.Dirt] = new Dirt(),
        [BlockId.Sand] = new Sand(),
        [BlockId.Stone] = new Stone(),
        [BlockId.Water] = new Water(),
        [BlockId.Glass] = new Glass(),
    };
}
