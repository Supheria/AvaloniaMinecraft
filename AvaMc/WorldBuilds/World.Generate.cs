using AvaMc.Blocks;

namespace AvaMc.WorldBuilds;

partial class World
{
    public static void Generate(Chunk chunk)
    {
        for (var x = 0; x < 16; x++)
        {
            for (var z = 0; z < 16; z++)
            {
                const int h = 64;
                for (var y = 0; y < h; y++)
                {
                    var type = BlockId.Air;
                    if (y == h - 1)
                        type = BlockId.Grass;
                    else if (y > (h - 3))
                        type = BlockId.Dirt;
                    else
                        type = BlockId.Stone;
                    var data = new BlockData() { BlockId = type };
                    chunk.SetData(new(x, y, z), data);
                }
            }
        }
    }
}
