using System.Collections.Generic;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Extensions;
using AvaMc.Gfx;
using AvaMc.Util;

// using Microsoft.Xna.Framework;

namespace AvaMc.WorldBuilds;

public sealed class Light
{
    const int LightMax = 15;

    private enum PropagateMode
    {
        Default,
        Sunlight,
    }

    private class LightChannelNode
    {
        public BlockWorldPosition Position { get; set; }
        public int Value { get; set; }
    }

    private class LightNode
    {
        public BlockWorldPosition Position { get; set; }
        public LightIbgrs Light { get; set; }
    }

    // TODO: for test
    public static void Add(World world, BlockWorldPosition position, LightIbgrs light)
    {
        var id = world.GetBlockId(position);
        if (!id.Block().Transparent)
            return;
        for (var i = 0; i < LightIbgrs.SunlightChannel; i++)
            AddChannel(world, position, light[i], i, PropagateMode.Default);
    }

    private static void AddChannel(
        World world,
        BlockWorldPosition position,
        int value,
        int channel,
        PropagateMode mode
    )
    {
        var light = world.GetAllLight(position);
        light[channel] = value;
        world.SetAllLight(position, light);
        var queue = new Queue<LightChannelNode>();
        var node = new LightChannelNode() { Position = position };
        queue.Enqueue(node);
        AddPropagate(world, queue, channel, mode);
    }

    // TODO: for test
    public static void Remove(World world, BlockWorldPosition position)
    {
        for (var i = 0; i < LightIbgrs.SunlightChannel; i++)
            RemoveChannel(world, position, i, PropagateMode.Default);
    }

    public static void UpdateAllLight(World world, BlockWorldPosition position)
    {
        var queue = new Queue<LightChannelNode>();
        for (var i = 0; i < LightIbgrs.ChannelCount; i++)
        {
            foreach (var direction in Direction.AllDirections)
            {
                var block = world.GetBlockId(position).Block();
                if (!block.Transparent)
                    return;
                var node = new LightChannelNode() { Position = position.ToNeighbor(direction) };
                queue.Enqueue(node);
            }
            var sunlight = i is 4;
            if (sunlight && position.Y > world.GetHighest(position))
            {
                world.SetSunlight(position, LightMax);
                var node = new LightChannelNode() { Position = position };
                queue.Enqueue(node);
            }
            var mode = sunlight ? PropagateMode.Sunlight : PropagateMode.Default;
            AddPropagate(world, queue, i, mode);
        }
    }

    private static void AddPropagate(
        World world,
        Queue<LightChannelNode> queue,
        int channel,
        PropagateMode mode
    )
    {
        while (queue.TryDequeue(out var node))
        {
            var light = world.GetAllLight(node.Position);
            var value = light[channel];
            foreach (var direction in Direction.AllDirectionsRevers)
            {
                var sunlightDown =
                    mode is PropagateMode.Sunlight && direction.Value is Direction.Type.Down;
                var nPos = node.Position.ToNeighbor(direction);
                var nData = world.GetBlockData(nPos);
                var nLight = nData.AllLight;
                var nValue = nLight[channel];
                var nBlock = nData.Id.Block();
                if (
                    (nValue != 0 || nBlock.Transparent)
                    && ((sunlightDown && nValue < value) || nValue + 1 < value)
                )
                {
                    var delta = sunlightDown ? 0 : -1;
                    nLight[channel] = light[channel] + delta;
                    world.SetAllLight(nPos, nLight);
                    var nNode = new LightChannelNode() { Position = nPos };
                    queue.Enqueue(nNode);
                }
            }
        }
    }

    public static void RemoveAllLight(World world, BlockWorldPosition position)
    {
        RemoveDefaultLight(world, position);
        RemoveChannel(world, position, LightIbgrs.SunlightChannel, PropagateMode.Sunlight);
    }

    private static void RemoveDefaultLight(World world, BlockWorldPosition position)
    {
        for (var i = 0; i < LightIbgrs.SunlightChannel; i++)
            RemoveChannel(world, position, i, PropagateMode.Default);
    }

    private static void RemoveChannel(
        World world,
        BlockWorldPosition position,
        int channel,
        PropagateMode mode
    )
    {
        var light = world.GetAllLight(position);
        light[channel] = 0;
        world.SetAllLight(position, light);

        var queue = new Queue<LightChannelNode>();
        var node = new LightChannelNode() { Position = position, Value = light[channel] };
        queue.Enqueue(node);
        var propQueue = new Queue<LightChannelNode>();
        RemovePropagate(world, queue, ref propQueue, channel, mode);
        AddPropagate(world, propQueue, channel, mode);
    }

    private static void RemovePropagate(
        World world,
        Queue<LightChannelNode> queue,
        ref Queue<LightChannelNode> propQueue,
        int channel,
        PropagateMode mode
    )
    {
        while (queue.TryDequeue(out var node))
        {
            var value = node.Value;
            foreach (var direction in Direction.AllDirections)
            {
                var nPos = node.Position.ToNeighbor(direction);
                var nLight = world.GetAllLight(nPos);
                var nValue = nLight[channel];
                var sunligihtDown =
                    mode is PropagateMode.Sunlight && direction.Value is Direction.Type.Down;
                if (nValue != 0 && (nValue < value || sunligihtDown))
                {
                    nLight[channel] = 0;
                    world.SetAllLight(nPos, nLight);
                    var nNode = new LightChannelNode() { Position = nPos, Value = nValue };
                    queue.Enqueue(nNode);
                }
                else if (nValue >= value)
                {
                    var nNode = new LightChannelNode() { Position = nPos };
                    propQueue.Enqueue(nNode);
                }
            }
        }
    }

    public static void ApplyAllLight(Chunk chunk)
    {
        var queue = new Queue<LightChannelNode>();

        for (var x = 0; x < ChunkData.ChunkSizeX; x++)
        {
            for (var z = 0; z < ChunkData.ChunkSizeZ; z++)
            {
                var cPos = chunk.CreatePosition(x, 0, z);
                var cH = chunk.GetHighest(cPos);
                if (cH < 0)
                    continue;
                for (var y = ChunkData.ChunkSizeY - 1; y >= cH; y--)
                {
                    cPos = chunk.CreatePosition(x, y, z);
                    chunk.SetSunlight(cPos, LightMax);
                    var wPos = cPos.ToWorld();
                    foreach (var direction in Direction.DirectionsXz)
                    {
                        var nPos = cPos.ToNeighbor(direction);
                        var nH = chunk.GetHighest(nPos);
                        if (wPos.Height >= nH)
                            continue;
                        var node = new LightChannelNode() { Position = wPos };
                        queue.Enqueue(node);
                        break;
                    }
                }
            }
        }

        AddPropagate(chunk.World, queue, LightIbgrs.SunlightChannel, PropagateMode.Sunlight);

        var borders = new List<LightNode>();
        for (var x = 0; x < ChunkData.ChunkSizeX; x++)
        {
            for (var z = 0; z < ChunkData.ChunkSizeZ; z++)
            {
                var pos = chunk.CreatePosition(x, -1, z).ToWorld();
                var light = chunk.GetAllLight(pos);
                if (light != LightIbgrs.Zero)
                {
                    var node = new LightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
                pos = chunk.CreatePosition(x, ChunkData.ChunkSizeY, z).ToWorld();
                light = chunk.GetAllLight(pos);
                if (light != LightIbgrs.Zero)
                {
                    var node = new LightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
            }
        }

        for (var x = 0; x < ChunkData.ChunkSizeX; x++)
        {
            for (var y = 0; y < ChunkData.ChunkSizeY; y++)
            {
                var pos = chunk.CreatePosition(x, y, -1).ToWorld();
                var light = chunk.GetAllLight(pos);
                if (light != LightIbgrs.Zero)
                {
                    var node = new LightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
                pos = chunk.CreatePosition(x, y, ChunkData.ChunkSizeZ).ToWorld();
                light = chunk.GetAllLight(pos);
                if (light != LightIbgrs.Zero)
                {
                    var node = new LightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
            }
        }

        for (var y = 0; y < ChunkData.ChunkSizeY; y++)
        {
            for (var z = 0; z < ChunkData.ChunkSizeZ; z++)
            {
                var pos = chunk.CreatePosition(-1, y, z).ToWorld();
                var light = chunk.GetAllLight(pos);
                if (light != LightIbgrs.Zero)
                {
                    var node = new LightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
                pos = chunk.CreatePosition(ChunkData.ChunkSizeX, y, z).ToWorld();
                light = chunk.GetAllLight(pos);
                if (light != LightIbgrs.Zero)
                {
                    var node = new LightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
            }
        }

        for (var i = 0; i < LightIbgrs.SunlightChannel; i++)
        {
            queue = [];
            foreach (var node in borders)
            {
                if (node.Light[i] is not 0)
                {
                    var cNode = new LightChannelNode() { Position = node.Position };
                    queue.Enqueue(cNode);
                }
            }
            AddPropagate(chunk.World, queue, i, PropagateMode.Default);
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
