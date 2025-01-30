using System.Collections.Generic;
using AvaMc.Blocks;
using AvaMc.Extensions;
using AvaMc.Util;
using Microsoft.Xna.Framework;

namespace AvaMc.WorldBuilds;

public class Light
{
    private class LightNode
    {
        public Vector3 Position { get; set; }
        int Value { get; set; }
    }

    public void Apply(Chunk chunk)
    {
        var heightmap = World.GetHeightmap(chunk);
        var sunlightQueue = new Queue<LightNode>();
        var torchlightQueue = new Queue<LightNode>();

        // propagate sunlight for this chunk
        for (var x = 0; x < Chunk.ChunkSizeX; x++)
        {
            for (var z = 0; z < Chunk.ChunkSizeZ; z++)
            {
                var h = heightmap.GetData(x, z);
                for (var y = Chunk.ChunkSizeY - 1; y >= 0; y--)
                {
                    var posC = new Vector3(x, y, z);
                    var posW = Vector3.Add(chunk.Position, posC);
                    if (posW.Y > h)
                    {
                        // check if this sunlight needs to be propagated in any
                        // N, E, S, W direction before queueing it
                        for (var d = Direction.North; d <= Direction.West; d++)
                        {
                            var dV = d.GetVector3();
                            var posCn = Vector3.Add(posC, dV);
                            var posWn = Vector3.Add(posW, dV);
                            var height = Chunk.InBounds(posCn)
                                ? heightmap.GetData(posCn.X, posCn.Z)
                                : chunk.World.HeightmapGet(posWn.Xz());
                            if (posW.Y < height)
                            {
                                sunlightQueue.Enqueue(new() { Position = posW });
                            }
                        }
                    }
                    
                    // enqueue torchlight emitting blocks
                    var block = Block.Blocks[chunk.GetBlockData(posC).BlockId];
                    if (block.CanEmitLight)
                    {
                        var light = block.GetTorchLight(chunk.World, posW);
                        
                    }
                }
            }
        }
    }
}
