using System;
using System.Collections.Generic;
using AvaMc.Coordinates;
using AvaMc.Extensions;
using AvaMc.Util;

namespace AvaMc.WorldBuilds;

partial class World
{
    private Heightmap? GetHeightmap(Vector2I offset)
    {
        if (!HeightmapInBounds(offset))
            return null;
        var index = HeightmapOffsetToIndex(offset);
        // var heightmaps = Heightmaps.AsSpan();
        return Heightmaps[index] ?? (Heightmaps[index] = new(offset));
    }

    public Heightmap? GetHeightmap(Vector3I chunkOffset)
    {
        var offset = chunkOffset.Xz();
        return GetHeightmap(offset);
    }

    private void SetHeightmap(BlockPosition position)
    {
        var offset = position.ToChunkOffset().Xz();
        var heightmap = GetHeightmap(offset);
        heightmap?.SetHeight(position);
    }

    public int GetHighest(BlockPosition position)
    {
        var offset = position.ToChunkOffset().Xz();
        var heightmap = GetHeightmap(offset);
        return heightmap?.GetHeight(position) ?? Heightmap.UnknownHeight;
    }

    public void UpdateHeightmap(BlockPosition position)
    {
        var offset = position.ToChunkOffset().Xz();
        var heightmap = GetHeightmap(offset);
        if (heightmap is null)
            return;
        var height = heightmap.GetHeight(position);
        if (position.Y > height)
            heightmap.SetHeight(position);
    }

    public unsafe void RecaculateHeightmap(BlockPosition position)
    {
        var offset = position.ToChunkOffset();
        if (!ChunkInBounds(offset))
            throw new ArgumentOutOfRangeException(nameof(position));
        var yMin = ChunksOrigin.Y * Chunk.ChunkSizeY;
        var yMax = (ChunksOrigin.Y + ChunksMagnitude) * Chunk.ChunkSizeY;
        for (var y = yMax; y >= yMin; y--)
        {
            position.Y = y;
            var block = GetBlockId(position).Block();
            if (block->Transparent)
                continue;
            SetHeightmap(position);
            return;
        }
        position.Y = Heightmap.UnknownHeight;
        SetHeightmap(position);
    }
}
