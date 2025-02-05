using System.Collections.Generic;
using AvaMc.Blocks;
using AvaMc.Extensions;
using AvaMc.Util;

// using Microsoft.Xna.Framework;

namespace AvaMc.WorldBuilds;

public sealed class Light
{
    private class LightNode
    {
        public Vector3I Position { get; set; }
        public int Value { get; set; }
    }

    public static void Add(World world, Vector3I pos, int light)
    {
        var data = world.GetBlockData(pos);
        if (!data.Block().Transparent)
            return;
        world.SetBlockData(pos, data.SetLight(light));
        var queue = new Queue<LightNode>();
        for (var i = 0; i < 4; i++)
        {
            var node = new LightNode() { Position = pos };
            queue.Enqueue(node);
            AddPropagate(world, queue, 0xF << (i * 4), i * 4);
        }
    }

    private static void AddPropagate(World world, Queue<LightNode> queue, int mask, int offset)
    {
        while (queue.TryDequeue(out var node))
        {
            var light = world.GetBlockData(node.Position).Light;
            foreach (var direction in Direction.AllDirections)
            {
                var nPos = Vector3I.Add(node.Position, direction.Vector3I);
                var nData = world.GetBlockData(nPos);
                var nBlock = nData.Block();
                var nLight = nData.Light;

                var test = ((nLight & mask) >> offset) + 1 < (light & mask) >> offset;
                if (nBlock.Transparent && test)
                {
                    var l = (nLight & ~mask) | ((((light & mask) >> offset) - 1) << offset);
                    world.SetBlockData(nPos, nData.SetLight(l));
                    node = new() { Position = nPos };
                    queue.Enqueue(node);
                }
            }
        }
    }

    public static void Remove(World world, Vector3I pos)
    {
        var data = world.GetBlockData(pos);
        var light = world.GetBlockData(pos).Light;
        world.SetBlockData(pos, data.SetLight(0));
        var queue = new Queue<LightNode>();
        var propQueue = new Queue<LightNode>();
        for (var i = 0; i < 4; i++)
        {
            var mask = 0xF << (i * 4);
            var offset = i * 4;
            var node = new LightNode() { Position = pos, Value = (light & mask) >> offset };
            queue.Enqueue(node);
            RemovePropagate(world, queue, ref propQueue, mask, offset);
            AddPropagate(world, propQueue, mask, offset);
        }
    }

    private static void RemovePropagate(
        World world,
        Queue<LightNode> queue,
        ref Queue<LightNode> propQueue,
        int mask,
        int offset
    )
    {
        while (queue.TryDequeue(out var node))
        {
            foreach (var direction in Direction.AllDirections)
            {
                var nPos = Vector3I.Add(node.Position, direction.Vector3I);
                var nLight = world.GetBlockData(nPos).Light;
                var nValue = (nLight & mask) >> offset;
                if ((nLight & mask) != 0 && nValue < node.Value)
                {
                    var data = world.GetBlockData(nPos);
                    world.SetBlockData(nPos, data.SetLight(nLight & ~mask));
                    node = new() { Position = nPos, Value = nValue };
                    queue.Enqueue(node);
                }
                else if (nValue > node.Value)
                {
                    node = new() { Position = nPos };
                    propQueue.Enqueue(node);
                }
            }
        }
    }

    //
    // public void Apply(Chunk chunk)
    // {
    //     var heightmap = World.GetHeightmap(chunk);
    //     var sunlightQueue = new Queue<LightNode>();
    //     var torchlightQueue = new Queue<LightNode>();
    //
    //     // propagate sunlight for this chunk
    //     for (var x = 0; x < Chunk.ChunkSizeX; x++)
    //     {
    //         for (var z = 0; z < Chunk.ChunkSizeZ; z++)
    //         {
    //             var h = heightmap.GetData(x, z);
    //             for (var y = Chunk.ChunkSizeY - 1; y >= 0; y--)
    //             {
    //                 var posC = new Vector3(x, y, z);
    //                 var posW = Vector3.Add(chunk.Position, posC);
    //                 if (posW.Y > h)
    //                 {
    //                     // check if this sunlight needs to be propagated in any
    //                     // N, E, S, W direction before queueing it
    //                     for (var d = Direction.North; d <= Direction.West; d++)
    //                     {
    //                         var dV = d.GetVector3();
    //                         var posCn = Vector3.Add(posC, dV);
    //                         var posWn = Vector3.Add(posW, dV);
    //                         var height = Chunk.InBounds(posCn)
    //                             ? heightmap.GetData(posCn.X, posCn.Z)
    //                             : chunk.World.HeightmapGet(posWn.Xz());
    //                         if (posW.Y < height)
    //                         {
    //                             sunlightQueue.Enqueue(new() { Position = posW });
    //                         }
    //                     }
    //                 }
    //
    //                 // enqueue torchlight emitting blocks
    //                 var block = Block.Blocks[chunk.GetData(posC).BlockId];
    //                 if (block.CanEmitLight)
    //                 {
    //                     var light = block.GetTorchLight(chunk.World, posW);
    //
    //                 }
    //             }
    //         }
    //     }
    // }
}
