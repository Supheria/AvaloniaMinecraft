using System.Collections.Generic;
using System.Numerics;
using AvaMc.Blocks;
using AvaMc.Gfx;
using AvaMc.Util;
using Hexa.NET.Utilities;

namespace AvaMc.WorldBuilds;

public sealed partial class BlockMesh
{
    public static void MeshSprite(
        ref ChunkMesh mesh,
        Vector3 position,
        AllLight allLight,
        Vector2 uvMin,
        Vector2 uvMax
    )
    {
        for (var i = 0; i < 4; i++)
        {
            var face = new Face(mesh.Indices.Count + i * 6, position);
            mesh.Faces.Add(face);
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
            mesh.Vertices.Add(vertex);
        }

        for (var i = 0; i < 24; i++)
            mesh.Indices.Add(SpriteIndices[i] + mesh.VertexCount);

        mesh.VertexCount += 8;
    }

    public static void MeshFace(
        ref ChunkMesh mesh,
        Vector3 position,
        AllLight allLight,
        Vector2 uvMin,
        Vector2 uvMax,
        bool transparent,
        Direction direction
    )
    {
        if (transparent)
        {
            var pos = Vector3.Add(FaceCenters[direction], position);
            var face = new Face(mesh.Indices.Count, pos);
            mesh.Faces.Add(face);
        }

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
            mesh.Vertices.Add(vertex);
        }

        for (var i = 0; i < 6; i++)
            mesh.Indices.Add(FaceIndices[i] + mesh.VertexCount);

        mesh.VertexCount += 4;
    }
}
