using System.Collections.Generic;
using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Gfx;
using AvaMc.Util;
using Hexa.NET.Utilities;

namespace AvaMc.WorldBuilds;

public sealed unsafe partial class BlockMesh
{
    // Block Block { get; set; }
    // Block Neighbor { get; set; }
    // public BlockData Block { get; set; }
    // public BlockData Neighbor { get; set; }
    // public Vector2 UvMin { get; set; }
    // public Vector2 UvMax { get; set; }
    // public Vector3 Position { get; set; }
    // public Direction Direction { get; set; } = Direction.Up;

    public static void MeshSprite(
        ref UnsafeList<BlockVertex> vertices,
        ref UnsafeList<uint> indices,
        ref UnsafeList<Face> faces,
        ref uint vertexCount,
        Vector3 position,
        AllLight allLight,
        Vector2 uvMin,
        Vector2 uvMax
    )
    {
        for (var i = 0; i < 4; i++)
        {
            var face = new Face(indices.Count + i * 6, position);
            faces.Add(face);
        }

        for (var i = 0; i < 8; i++)
        {
            var index = i * 3;
            var x = position.X + CubeVertices[index++];
            var y = position.Y + CubeVertices[index++];
            var z = position.Z + CubeVertices[index];
            index = (i % 4) * 2;
            var u = CubeUv(index++, uvMin.X, uvMax.X);
            var v = CubeUv(index, uvMin.Y, uvMax.Y);

            var light = allLight.MakeFinal(Direction.Up);
            var vertex = new BlockVertex(new(x, y, z), new(u, v), light);
            vertices.Add(vertex);
        }

        for (var i = 0; i < 24; i++)
            indices.Add(SpriteIndices[i] + vertexCount);

        vertexCount += 8;
    }

    public static void MeshTransparentFace(
        ref UnsafeList<BlockVertex> vertices,
        ref UnsafeList<uint> indices,
        ref UnsafeList<Face> faces,
        ref uint vertexCount,
        Vector3 position,
        AllLight allLight,
        Vector2 uvMin,
        Vector2 uvMax,
        Direction direction
    )
    {
        var pos = Vector3.Add(FaceCenters[direction], position);
        var face = new Face(indices.Count, pos);
        faces.Add(face);

        // TODO: only shorten if blocks other neighbors are not liquid
        var yScale = 1f;
        for (var i = 0; i < 4; i++)
        {
            var index = CubeIndices[(direction * 6) + UniqueIndices[i]] * 3;
            var x = position.X + CubeVertices[index++];
            var y = position.Y + CubeVertices[index++] * yScale;
            var z = position.Z + CubeVertices[index];
            index = i * 2;
            var u = CubeUv(index++, uvMin.X, uvMax.X);
            var v = CubeUv(index, uvMin.Y, uvMax.Y);

            var light = allLight.MakeFinal(Direction.Up);
            var vertex = new BlockVertex(new(x, y, z), new(u, v), light);
            vertices.Add(vertex);
        }

        for (var i = 0; i < 6; i++)
            indices.Add(vertexCount + FaceIndices[i]);

        vertexCount += 4;
    }

    public static void MeshSolidFace(
        ref UnsafeList<BlockVertex> vertices,
        ref UnsafeList<uint> indices,
        ref uint vertexCount,
        Vector3 position,
        AllLight allLight,
        Vector2 uvMin,
        Vector2 uvMax,
        Direction direction
    )
    {
        for (var i = 0; i < 4; i++)
        {
            var index = CubeIndices[direction * 6 + UniqueIndices[i]] * 3;
            var x = position.X + CubeVertices[index++];
            var y = position.Y + CubeVertices[index++];
            var z = position.Z + CubeVertices[index];
            index = i * 2;
            var u = CubeUv(index++, uvMin.X, uvMax.X);
            var v = CubeUv(index, uvMin.Y, uvMax.Y);

            var light = allLight.MakeFinal(Direction.Up);
            var vertex = new BlockVertex(new(x, y, z), new(u, v), light);
            vertices.Add(vertex);
        }

        for (var i = 0; i < 6; i++)
            indices.Add(vertexCount + FaceIndices[i]);

        vertexCount += 4;
    }
}
