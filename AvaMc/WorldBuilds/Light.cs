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
        public BlockPosition Position { get; set; }
        public int Value { get; set; }
    }

    private class TorchLightNode
    {
        public BlockPosition Position { get; set; }
        public TorchLight Light { get; set; }
    }

    // TODO: for test
    public static void Add(World world, BlockPosition position, TorchLight torchLight)
    {
        var id = world.GetBlockId(position);
        if (!id.Block().Transparent)
            return;
        for (var i = 0; i < TorchLight.ChannelCount; i++)
            AddChannel(world, position, torchLight[i], i, PropagateMode.Default);
    }

    // TODO: for test
    public static void Remove(World world, BlockPosition position)
    {
        for (var i = 0; i < AllLight.SunlightChannel; i++)
            RemoveChannel(world, position, i, PropagateMode.Default);
    }

    private static void AddChannel(
        World world,
        BlockPosition position,
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

    public static void UpdateAllLight(World world, BlockPosition position)
    {
        var queue = new Queue<LightChannelNode>();
        for (var i = 0; i < AllLight.ChannelCount; i++)
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
                var nBlock = nData.BlockId.Block();
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

    public static void RemoveAllLight(World world, BlockPosition position)
    {
        RemoveTorchLight(world, position);
        RemoveChannel(world, position, AllLight.SunlightChannel, PropagateMode.Sunlight);
    }

    private static void RemoveTorchLight(World world, BlockPosition position)
    {
        for (var i = 0; i < TorchLight.ChannelCount; i++)
            RemoveChannel(world, position, i, PropagateMode.Default);
    }

    private static void RemoveChannel(
        World world,
        BlockPosition position,
        int channel,
        PropagateMode mode
    )
    {
        var light = world.GetAllLight(position);
        var value = light[channel];
        light[channel] = 0;
        world.SetAllLight(position, light);

        var queue = new Queue<LightChannelNode>();
        var node = new LightChannelNode() { Position = position, Value = value };
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
    
    public static void AddTorchLight(World world, BlockPosition position, TorchLight torchLight)
    {
        var block = world.GetBlockId(position).Block();
        if (!block.Transparent)
            return;
        for (var i = 0; i < 4; i++)
            AddChannel(world, position, torchLight[i], i, PropagateMode.Default);
    }

    public static void ApplyAllLight(Chunk chunk)
    {
        var queue = new Queue<LightChannelNode>();

        for (var x = 0; x < Chunk.ChunkSizeX; x++)
        {
            for (var z = 0; z < Chunk.ChunkSizeZ; z++)
            {
                var pos = chunk.CreatePosition(x, 0, z);
                var h = chunk.GetHighest(pos);

                for (var y = Chunk.ChunkSizeY - 1; y >= 0; y--)
                {
                    pos = chunk.CreatePosition(x, y, z);
                    var wPos = pos.IntoWorld();
                    if (wPos.Height <= h)
                        continue;
                    chunk.SetSunlight(pos, LightMax);
                    foreach (var direction in Direction.DirectionsXz)
                    {
                        var nPos = pos.ToNeighbor(direction);
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

        AddPropagate(chunk.World, queue, AllLight.SunlightChannel, PropagateMode.Sunlight);

        var borders = new List<TorchLightNode>();
        for (var x = 0; x < Chunk.ChunkSizeX; x++)
        {
            for (var z = 0; z < Chunk.ChunkSizeZ; z++)
            {
                var pos = chunk.CreatePosition(x, -1, z).IntoWorld();
                var light = chunk.World.GetTorchLight(pos);
                if (light != AllLight.Zero)
                {
                    var node = new TorchLightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
                pos = chunk.CreatePosition(x, Chunk.ChunkSizeY, z).IntoWorld();
                light = chunk.World.GetTorchLight(pos);
                if (light != AllLight.Zero)
                {
                    var node = new TorchLightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
            }
        }

        for (var x = 0; x < Chunk.ChunkSizeX; x++)
        {
            for (var y = 0; y < Chunk.ChunkSizeY; y++)
            {
                var pos = chunk.CreatePosition(x, y, -1).IntoWorld();
                var light = chunk.World.GetTorchLight(pos);
                if (light != AllLight.Zero)
                {
                    var node = new TorchLightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
                pos = chunk.CreatePosition(x, y, Chunk.ChunkSizeZ).IntoWorld();
                light = chunk.World.GetTorchLight(pos);
                if (light != AllLight.Zero)
                {
                    var node = new TorchLightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
            }
        }

        for (var y = 0; y < Chunk.ChunkSizeY; y++)
        {
            for (var z = 0; z < Chunk.ChunkSizeZ; z++)
            {
                var pos = chunk.CreatePosition(-1, y, z).IntoWorld();
                var light = chunk.World.GetTorchLight(pos);
                if (light != AllLight.Zero)
                {
                    var node = new TorchLightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
                pos = chunk.CreatePosition(Chunk.ChunkSizeX, y, z).IntoWorld();
                light = chunk.World.GetTorchLight(pos);
                if (light != AllLight.Zero)
                {
                    var node = new TorchLightNode() { Position = pos, Light = light };
                    borders.Add(node);
                }
            }
        }

        for (var i = 0; i < TorchLight.ChannelCount; i++)
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
}
