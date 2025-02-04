using System;
using System.Collections.Generic;
using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Extensions;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

public sealed class WorldGenerator
{
    const int WaterLevel = 64;
    private delegate BlockData Get(Chunk chunk, int x, int y, int z);
    private delegate void Set(Chunk chunk, int x, int y, int z, BlockData data);
    int Seed { get; }

    public WorldGenerator(int seed)
    {
        Seed = seed;
    }

    private bool RandomChance(Random random, double chance)
    {
        var rand = random.NextDouble();
        return rand <= chance;
    }

    private float Radial2I(Vector2I c, Vector2I r, Vector2I v)
    {
        var div = Vector2.Subtract(c.ToNumerics(), v.ToNumerics()) / r.ToNumerics().Length();
        return div.Length();
    }

    private float Radial3I(Vector3I c, Vector3I r, Vector3I v)
    {
        var div = Vector3.Subtract(c.ToNumerics(), v.ToNumerics()) / r.ToNumerics().Length();
        return div.Length();
    }

    private int GetChunkRandomSeed(Chunk chunk)
    {
        var h = 0L;
        var v = chunk.Offset.ToNumerics();
        for (var i = 0; i < 3; i++)
        {
            h ^= (int)v[i] + 0x9e3779b9 + (h << 6) + (h >> 2);
        }
        return (int)(Seed + h);
    }

    private Get GetBlockData { get; } =
        (chunk, x, y, z) =>
        {
            var p = new Vector3I(x, y, z);
            if (Chunk.InBounds(p))
                return chunk.GetBlockData(p);
            else
                return chunk.GetBlockDataInOtherChunk(p);
        };

    private Set SetBlockData { get; } =
        (chunk, x, y, z, data) =>
        {
            var p = new Vector3I(x, y, z);
            if (Chunk.InBounds(p))
                chunk.SetData(p, data);
            else
                chunk.SetBlockDataInOtherChunk(p, data);
        };

    private void Tree(Random random, Chunk chunk, Get get, Set set, int x, int y, int z)
    {
        var h = random.Next(3, 5);
        for (var yy = y; yy <= y + h; yy++)
            set(chunk, x, yy, z, new() { BlockId = BlockId.Log });

        var lh = random.Next(2, 3);
        for (var xx = x - 2; xx <= x + 2; xx++)
        {
            for (var zz = z - 2; zz <= z + 2; zz++)
            {
                for (var yy = y + h; yy <= y + h + 1; yy++)
                {
                    var bx = xx == x - 2 || xx == x + 2;
                    var bz = zz == z - 2 || zz == z + 2;
                    var corner = bx && bz;
                    if (
                        (!(xx == x && zz == z) || yy > y + h)
                        && !(corner && yy == y + h + 1 && RandomChance(random, 0.4f))
                    )
                    {
                        set(chunk, xx, yy, zz, new() { BlockId = BlockId.Leaves });
                    }
                }
            }
        }

        for (var xx = x - 1; xx <= x + 1; xx++)
        {
            for (var zz = z - 1; zz <= z + 1; zz++)
            {
                for (var yy = y + h + 2; yy <= y + h + lh; yy++)
                {
                    var bx = xx == x - 1 || xx == x + 1;
                    var bz = zz == z - 1 || zz == z + 1;
                    var corner = bx && bz;
                    if (!(corner && yy == y + h + lh && RandomChance(random, 0.8)))
                    {
                        set(chunk, xx, yy, zz, new() { BlockId = BlockId.Leaves });
                    }
                }
            }
        }
    }

    private void Flowers(Random random, Chunk chunk, Get get, Set set, int x, int y, int z)
    {
        var flower = new BlockData()
        {
            BlockId = RandomChance(random, 0.6) ? BlockId.Rose : BlockId.Buttercup,
        };
        var l = random.Next(1, 4);
        var h = random.Next(1, 4);
        for (var xx = x - l; xx <= x + l; xx++)
        {
            for (var zz = z - h; zz <= z + h; zz++)
            {
                var under = get(chunk, xx, y, zz);
                if (under.BlockId is BlockId.Grass && RandomChance(random, 0.5))
                {
                    set(chunk, xx, y + 1, zz, flower);
                }
            }
        }
    }

    private void Orevein(
        Random random,
        Chunk chunk,
        Get get,
        Set set,
        int x,
        int y,
        int z,
        BlockId id
    )
    {
        var h = random.Next(1, y - 4);
        var s = id switch
        {
            BlockId.Coal => 4,
            BlockId.Copper => 1,
            _ => 0,
        };
        var l = random.Next(1, s);
        var w = random.Next(1, s);
        var i = random.Next(1, s);

        for (var xx = x - l; xx <= x + l; xx++)
        {
            for (var zz = z - w; zz <= z + w; zz++)
            {
                for (var yy = h - i; yy <= h + i; yy++)
                {
                    var d = 1 - Radial3I(new(x, h, z), new(l, w, i), new(xx, yy, zz));
                    var data = get(chunk, xx, yy, zz);
                    if (data.BlockId is BlockId.Stone && RandomChance(random, 0.2 + d * 0.7))
                        set(chunk, xx, yy, zz, new() { BlockId = id });
                }
            }
        }
    }

    private void LavaPool(Random random, Chunk chunk, Get get, Set set, int x, int y, int z)
    {
        var h = y - 1;
        var s = random.Next(1, 5);
        var l = random.Next(s - 1, s + 1);
        var w = random.Next(s - 1, s + 1);
        for (var xx = x - l; xx <= x + l; xx++)
        {
            for (var zz = z - w; zz <= z + w; zz++)
            {
                var d = 1 - Radial2I(new(x, z), new(l + 1, w + 1), new(xx, zz));
                var allow = true;
                for (var i = -1; i <= 1; i++)
                {
                    for (var j = -1; j <= 1; j++)
                    {
                        var data = get(chunk, xx + i, h, zz + j);
                        if (data.BlockId is BlockId.Lava || !Block.Blocks[data.BlockId].Transparent)
                            continue;
                        allow = false;
                        break;
                    }
                }

                if (!allow)
                    continue;
                if (RandomChance(random, 0.2 + d * 0.95))
                    set(chunk, xx, h, zz, new() { BlockId = BlockId.Lava });
            }
        }
    }

    public void Generate(Chunk chunk)
    {
        var seed = GetChunkRandomSeed(chunk);
        var random = new Random(seed);

        var offsets = new[]
        {
            new OctaveNoise(8, 1),
            new OctaveNoise(8, 2),
            new OctaveNoise(8, 3),
            new OctaveNoise(8, 4),
        };
        var combineds = new[]
        {
            new CombinedNoise(offsets[0], offsets[1]),
            new CombinedNoise(offsets[2], offsets[3]),
        };
        var biomeNoise = new OctaveNoise(6, 0);
        var oreNoise = new OctaveNoise(6, 1);

        for (var x = 0; x < 16; x++)
        {
            for (var z = 0; z < 16; z++)
            {
                var wx = chunk.Position.X + x;
                var wz = chunk.Position.Z + z;
                var baseScale = 1.3f;
                var hl = (int)(
                    (combineds[0].Compute(Seed, wx * baseScale, wz * baseScale) / 6.0f) - 4.0f
                );
                var hh = (int)(
                    (combineds[1].Compute(Seed, wx * baseScale, wz * baseScale) / 5.0f) + 6.0f
                );

                var t = biomeNoise.Compute(Seed, wx, wz);
                var r = oreNoise.Compute(Seed, wx * 4, wz * 4) / 32;

                var hr = t > 0 ? hl : Math.Max(hl, hh);
                var h = hr + WaterLevel;

                var biome =
                    h < WaterLevel ? Biome.Ocean
                    : (t < 0.08f && h < WaterLevel + 2) ? Biome.Beach
                    : Biome.Plains;

                var d = r * 1.4f + 5;
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
                    else if (y > h - d)
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

                for (var y = h; y < WaterLevel; y++)
                {
                    var data = new BlockData() { BlockId = BlockId.Water };
                    chunk.SetData(new(x, y, z), data);
                }

                if (RandomChance(random, 0.004))
                    Orevein(random, chunk, GetBlockData, SetBlockData, x, h, z, BlockId.Coal);
                if (RandomChance(random, 0.004))
                    Orevein(random, chunk, GetBlockData, SetBlockData, x, h, z, BlockId.Copper);
                if (
                    biome is not Biome.Ocean
                    && h < WaterLevel + 3
                    && t < 0.1f
                    && RandomChance(random, 0.005)
                )
                    LavaPool(random, chunk, GetBlockData, SetBlockData, x, h, z);
                if (biome is Biome.Plains && RandomChance(random, 0.005))
                    Tree(random, chunk, GetBlockData, SetBlockData, x, h, z);
                if (biome is Biome.Plains && RandomChance(random, 0.0015))
                    Flowers(random, chunk, GetBlockData, SetBlockData, x, h, z);
            }
        }

        var loaded = new List<WorldUnloadedData>();
        foreach (var unloaded in chunk.World.UnloadedData)
        {
            if (chunk.Offset != unloaded.Position.WorldBlockPosToChunkOffset())
                continue;
            chunk.SetData(unloaded.Position.BlockPosWorldToChunk(), unloaded.Data);

            loaded.Add(unloaded);
        }
        loaded.ForEach(d => chunk.World.UnloadedData.Remove(d));
    }
}
