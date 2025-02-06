using System.Collections.Generic;
using AvaMc.Blocks;
using AvaMc.Extensions;
using AvaMc.Gfx;
using AvaMc.Util;

// using Microsoft.Xna.Framework;

namespace AvaMc.WorldBuilds;

public sealed class Light
{
    private class LightNode
    {
        public Vector3I Position { get; set; }
        public float Value { get; set; }
    }

    public static void Add(World world, Vector3I pos, LightRgbi light)
    {
        var id = world.GetBlockId(pos);
        if (!id.Block().Transparent)
            return;
        world.SetBlockLight(pos, light);
        // var queue = new Queue<LightNode>();
        var queue = new Queue<LightNode>();
        for (var i = 0; i < LightRgbi.ChannelCount; i++)
        {
            var node = new LightNode() { Position = pos };
            queue.Enqueue(node);
            AddPropagate(world, queue, i);
        }
    }

    private static void AddPropagate(World world, Queue<LightNode> queue, int channel)
    {
        while (queue.TryDequeue(out var node))
        {
            var light = world.GetBlockLight(node.Position);
            foreach (var direction in Direction.AllDirections)
            {
                var nPos = Vector3I.Add(node.Position, direction.Vector3I);
                var nData = world.GetBlockAllData(nPos);
                var nBlock = nData.Id.Block();
                var nLight = nData.Light;

                var test = nLight[channel] + 1 < light[channel];
                if (nBlock.Transparent && test)
                {
                    nLight[channel] = light[channel] - 1;
                    world.SetBlockLight(nPos, nLight);
                    var newNode = new LightNode() { Position = nPos };
                    queue.Enqueue(newNode);
                }
            }
        }
    }

    public static void Remove(World world, Vector3I pos)
    {
        var light = world.GetBlockLight(pos);
        world.SetBlockLight(pos, LightRgbi.Zero);
        var queue = new Queue<LightNode>();
        var propQueue = new Queue<LightNode>();
        for (var i = 0; i < LightRgbi.ChannelCount; i++)
        {
            var node = new LightNode() { Position = pos, Value = light[i] };
            queue.Enqueue(node);
            RemovePropagate(world, queue, ref propQueue, i);
            AddPropagate(world, propQueue, i);
        }
    }

    private static void RemovePropagate(
        World world,
        Queue<LightNode> queue,
        ref Queue<LightNode> propQueue,
        int channel
    )
    {
        while (queue.TryDequeue(out var node))
        {
            foreach (var direction in Direction.AllDirections)
            {
                var nPos = Vector3I.Add(node.Position, direction.Vector3I);
                var nLight = world.GetBlockLight(nPos);
                var nValue = nLight[channel];
                if (nValue > 0 && nValue < node.Value)
                {
                    nLight[channel] = 0;
                    world.SetBlockLight(nPos, nLight);
                    var newNode = new LightNode() { Position = nPos, Value = nValue };
                    queue.Enqueue(newNode);
                }
                else if (nValue > node.Value)
                {
                    var newNode = new LightNode()  { Position = nPos };
                    propQueue.Enqueue(newNode);
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
