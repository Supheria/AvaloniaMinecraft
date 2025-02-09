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

    private void SetHeightmap(BlockPosition position)
    {
        var offset = position.ToChunkOffsetXz();
        var heightmap = GetHeightmap(offset);
        heightmap.SetHeight(position);
    }

    public int GetHighest(BlockPosition position)
    {
        var offset = position.ToChunkOffset().Xz();
        if (!InBounds(offset))
            return Heightmap.UnknownHeight;
        var heightmap = GetHeightmap(offset);
        return heightmap.GetHeight(position);
    }

    public bool UpdateHeightmap(BlockPosition position)
    {
        var offset = position.ToChunkOffsetXz();
        var heightmap = GetHeightmap(offset);
        var height = heightmap.GetHeight(position);
        if (position.Y <= height)
            return false;
        heightmap.SetHeight(position);
        return true;
    }

    public void RecaculateHeightmap(BlockPosition position)
    {
        var offset = position.ToChunkOffset();
        if (!InBounds(offset))
            throw new ArgumentOutOfRangeException(nameof(position));
        var yMin = ChunksOrigin.Y * ChunkData.ChunkSizeY;
        var yMax = (ChunksOrigin.Y + ChunksMagnitude) * ChunkData.ChunkSizeY;
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
