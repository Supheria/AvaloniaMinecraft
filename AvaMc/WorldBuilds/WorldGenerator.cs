using System;
using System.Collections.Generic;
using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Extensions;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

public sealed class WorldGenerator
{
    const int WaterLevel = 64;
    private delegate BlockId Get(Chunk chunk, int x, int y, int z);
    private delegate void Set(Chunk chunk, int x, int y, int z, BlockId id);
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
        var code = chunk.GetOffsetHashCode();
        return (int)(Seed + code);
    }

    private Get GetBlockData { get; } =
        (chunk, x, y, z) =>
        {
            var p = chunk.CreatePosition(x, y, z);
            return chunk.GetBlockId(p);
        };

    private Set SetBlockData { get; } =
        (chunk, x, y, z, id) =>
        {
            var p = chunk.CreatePosition(x, y, z);
            chunk.SetBlockId(p, id);
        };

    private void Tree(Random random, Chunk chunk, Get get, Set set, int x, int y, int z)
    {
        var under = get(chunk, x, y - 1, z);
        if (under is not BlockId.Grass && under is not BlockId.Dirt)
            return;

        var h = random.Next(3, 5);
        for (var yy = y; yy <= y + h; yy++)
            set(chunk, x, yy, z, BlockId.Log);

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
                        set(chunk, xx, yy, zz, BlockId.Leaves);
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
                        set(chunk, xx, yy, zz, BlockId.Leaves);
                    }
                }
            }
        }
    }

    private void Flowers(Random random, Chunk chunk, Get get, Set set, int x, int y, int z)
    {
        var flower = RandomChance(random, 0.6) ? BlockId.Rose : BlockId.Buttercup;
        var s = random.Next(2, 6);
        var l = random.Next(s - 1, s + 1);
        var h = random.Next(s - 1, s + 1);
        for (var xx = x - l; xx <= x + l; xx++)
        {
            for (var zz = z - h; zz <= z + h; zz++)
            {
                var under = get(chunk, xx, y, zz);
                if (under is BlockId.Grass && RandomChance(random, 0.5))
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
            BlockId.Coal => random.Next(2, 4),
            BlockId.Copper => random.Next(1, 3),
            _ => 0,
        };
        var l = random.Next(s - 1, s + 1);
        var w = random.Next(s - 1, s + 1);
        var i = random.Next(s - 1, s + 1);

        for (var xx = x - l; xx <= x + l; xx++)
        {
            for (var zz = z - w; zz <= z + w; zz++)
            {
                for (var yy = h - i; yy <= h + i; yy++)
                {
                    var d = 1 - Radial3I(new(x, h, z), new(l, w, i), new(xx, yy, zz));
                    var target = get(chunk, xx, yy, zz);
                    if (target is BlockId.Stone && RandomChance(random, 0.2 + d * 0.7))
                        set(chunk, xx, yy, zz, id);
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
                        var id = get(chunk, xx + i, h, zz + j);
                        if (id is BlockId.Lava || !id.Block().Transparent)
                            continue;
                        allow = false;
                        break;
                    }
                }

                if (!allow)
                    continue;
                if (RandomChance(random, 0.2 + d * 0.95))
                    set(chunk, xx, h, zz, BlockId.Lava);
            }
        }
    }

    public void Generate(Chunk chunk)
    {
        var seed = GetChunkRandomSeed(chunk);
        var random = new Random(seed);

        // chunk.SetBlockData(new(2, 2, 2), new() { BlockId = BlockId.Stone });
        // return;

        //TODO
        for (var x = 0; x < ChunkData.ChunkSizeX; x++)
        {
            for (var y = 0; y < ChunkData.ChunkSizeY; y++)
            {
                for (var z = 0; z < ChunkData.ChunkSizeZ; z++)
                {
                    var p = chunk.CreatePosition(x, 0, z);
                    var w = p.ToWorld();
                    // if (w.Y > 60 && w.Y < 70)
                    BlockId id;
                    if (w.Y > 64)
                    {
                        continue;
                    }
                    else if (w.Y > 63)
                    {
                        id = BlockId.Grass;
                    }
                    else if (w.Y > 60)
                    {
                        id = BlockId.Dirt;
                    }
                    else
                    {
                        id = BlockId.Stone;
                    }

                    chunk.SetBlockId(p, id);
                }
            }
        }
        return;

        var offsets = new[]
        {
            new OctaveNoise(8, 1),
            new OctaveNoise(8, 2),
            new OctaveNoise(8, 3),
            new OctaveNoise(8, 4),
            new OctaveNoise(8, 5),
            new OctaveNoise(8, 6),
        };
        var combineds = new[]
        {
            new CombinedNoise(offsets[0], offsets[1]),
            new CombinedNoise(offsets[2], offsets[3]),
            new CombinedNoise(offsets[4], offsets[5]),
        };
        var biomeNoise = new OctaveNoise(6, 0);
        var oreNoise = new OctaveNoise(6, 1);

        for (var x = 0; x < 16; x++)
        {
            for (var z = 0; z < 16; z++)
            {
                var p = chunk.CreatePosition(x, 0, z);
                var w = p.ToWorld();
                var wx = w.X;
                var wz = w.Z;
                var baseScale = 1.3f;
                var hl = (int)(
                    (combineds[0].Compute(Seed, wx * baseScale, wz * baseScale) / 6.0f) - 4.0f
                );
                var hh = (int)(
                    (combineds[1].Compute(Seed, wx * baseScale, wz * baseScale) / 5.0f) + 6.0f
                );

                var t = biomeNoise.Compute(Seed, wx, wz);
                var r = oreNoise.Compute(Seed, wx / 4f, wz / 4f) / 32;

                var hr = t > 0 ? hl : Math.Max(hl, hh);
                var h = hr + WaterLevel;

                Biome biome;
                if (h < WaterLevel)
                    biome = Biome.Ocean;
                else if (t < 0.08f && h < WaterLevel + 2)
                    biome = Biome.Beach;
                // TODO
                else if (false)
                    biome = Biome.Mountain;
                else
                    biome = Biome.Plains;

                if (biome is Biome.Mountain)
                    h += (int)(r + (-t / 12)) * 2 + 2;

                var d = r * 1.4f + 5;

                var topBlock = BlockId.Air;
                switch (biome)
                {
                    case Biome.Ocean:
                        if (r > 0.8f)
                            topBlock = BlockId.Gravel;
                        else if (r > 0.3f)
                            topBlock = BlockId.Sand;
                        else if (r > 0.15f && t < 0.08f)
                            topBlock = BlockId.Clay;
                        else
                            topBlock = BlockId.Dirt;
                        break;
                    case Biome.Beach:
                        topBlock = BlockId.Sand;
                        break;
                    case Biome.Plains:
                        if (t > 4f && r > 0.78f)
                            topBlock = BlockId.Gravel;
                        else
                            topBlock = BlockId.Grass;
                        break;
                    case Biome.Mountain:
                        if (r > 0.8f)
                            topBlock = BlockId.Gravel;
                        else if (r > 0.7f)
                            topBlock = BlockId.Dirt;
                        else
                            topBlock = BlockId.Stone;
                        break;
                }

                for (var y = 0; y < h; y++)
                {
                    var id = BlockId.Air;
                    if (y == h - 1)
                        id = topBlock;
                    else if (y > h - d)
                    {
                        if (topBlock is BlockId.Grass)
                            id = BlockId.Dirt;
                        else
                            id = topBlock;
                    }
                    else
                        id = BlockId.Stone;
                    chunk.SetBlockId(x, y, z, id);
                }

                for (var y = h; y < WaterLevel; y++)
                {
                    chunk.SetBlockId(x, y, z, BlockId.Water);
                }

                if (RandomChance(random, 0.02))
                    Orevein(random, chunk, GetBlockData, SetBlockData, x, h, z, BlockId.Coal);
                if (RandomChance(random, 0.02))
                    Orevein(random, chunk, GetBlockData, SetBlockData, x, h, z, BlockId.Copper);
                if (
                    biome is not Biome.Ocean
                    && h < WaterLevel + 3
                    && t < 0.1f
                    && RandomChance(random, 0.001)
                )
                    LavaPool(random, chunk, GetBlockData, SetBlockData, x, h, z);
                if (biome is Biome.Plains && RandomChance(random, 0.005))
                    Tree(random, chunk, GetBlockData, SetBlockData, x, h, z);
                if (biome is Biome.Plains && RandomChance(random, 0.0085))
                    Flowers(random, chunk, GetBlockData, SetBlockData, x, h, z);
            }
        }

        foreach (var (pos, id) in chunk.World.UnloadedBlockIds)
            chunk.SetBlockId(pos, id);
    }
}
