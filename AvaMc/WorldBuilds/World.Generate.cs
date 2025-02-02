using System;
using AvaMc.Blocks;

namespace AvaMc.WorldBuilds;

partial class World
{
    const int WaterLevel = 64;

    public static void Generate(Chunk chunk)
    {
        var seed = 20134L;
        var offsets = new[] { new Noise(8, 1), new Noise(8, 2), new Noise(8, 3), new Noise(8, 4) };
        var combineds = new[]
        {
            new CombinedNoise(offsets[0], offsets[1]),
            new CombinedNoise(offsets[2], offsets[3]),
        };
        var biomeNoise = new Noise(6, 0);

        for (var x = 0; x < 16; x++)
        {
            for (var z = 0; z < 16; z++)
            {
                var wx = chunk.Position.X + x;
                var wz = chunk.Position.Z + z;
                var baseScale = 1.3f;
                var h1 = (int)((combineds[0].Compute(seed, wx * baseScale, wz * baseScale) / 6.0f) - 4.0f);
                var h2 = (int)((combineds[1].Compute(seed, wx * baseScale, wz * baseScale) / 5.0f) + 6.0f);

                var t = biomeNoise.Compute(seed, wx, wz);
                var hr = t > 0 ? h1 : Math.Max(h1, h2);
                var h = hr + WaterLevel;

                var biome =
                    h < WaterLevel ? Biome.Ocean
                    : (t < 0.08f && h < WaterLevel + 2) ? Biome.Beach
                    : Biome.Plains;

                for (var y = 0; y < h; y++)
                {
                    var type = BlockId.Air;
                    if (y == h - 1)
                    {
                        switch (biome)
                        {
                            case Biome.Ocean:
                                type = t > 0.03f ? BlockId.Dirt : BlockId.Sand;
                                break;
                            case Biome.Beach:
                                type = BlockId.Sand;
                                break;
                            case Biome.Plains:
                                type = BlockId.Grass;
                                break;
                        }
                    }
                    else if (y > h - 4)
                    {
                        type = biome is Biome.Beach ? BlockId.Sand : BlockId.Dirt;
                    }
                    else
                    {
                        type = BlockId.Stone;
                    }
                    var data = new BlockData() { BlockId = type };
                    chunk.SetData(new(x, y, z), data);
                }
            }
        }
    }
}
