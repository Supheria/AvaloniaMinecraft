using System;
using System.Collections.Generic;
using AvaMc.Coordinates;
using AvaMc.Extensions;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

partial class World
{
    Dictionary<Vector2I, Heightmap> Heightmaps { get; } = [];

    private Heightmap GetHeightmap(Vector2I offset)
    {
        if (Heightmaps.TryGetValue(offset, out var heightmap))
            return heightmap;
        Heightmaps[offset] = new();
        return Heightmaps[offset];
    }
    
    public Heightmap GetHeightmap(ChunkOffset chunkOffset)
    {
        var offset = chunkOffset.ToInernal().Xz();
        return GetHeightmap(offset);
    }

    private void SetHeightmap(BlockWorldPosition position)
    {
        var offset = position.ToChunkOffsetXz();
        var heightmap = GetHeightmap(offset);
        heightmap.Set(position);
    }

    public int GetHighest(BlockWorldPosition position)
    {
        var offset = position.ToChunkOffset().Xz();
        if (!InBounds(offset))
            return Heightmap.UnknownHeight;
        var heightmap = GetHeightmap(offset);
        return heightmap.Get(position);
    }

    public bool UpdateHeightmap(BlockWorldPosition position)
    {
        var offset = position.ToChunkOffsetXz();
        var heightmap = GetHeightmap(offset);
        var height = heightmap.Get(position);
        if (position.Y <= height)
            return false;
        heightmap.Set(position);
        return true;
    }

    public void RecaculateHeightmap(BlockWorldPosition position)
    {
        var offset = position.ToChunkOffset();
        if (!InBounds(offset))
            throw new ArgumentOutOfRangeException(nameof(position));
        var yMin = ChunksOrigin.Y - ChunksMagnitude / 2 * ChunkData.ChunkSizeY;
        var yMax = ChunksOrigin.Y + ChunksMagnitude / 2 * ChunkData.ChunkSizeY;
        for (var y = yMax; y >= yMin; y--)
        {
            position.Y = y;
            var block = GetBlockId(position).Block();
            if (block.Transparent)
                continue;
            SetHeightmap(position);
            return;
        }
        position.Y = Heightmap.UnknownHeight;
        SetHeightmap(position);
    }
}
