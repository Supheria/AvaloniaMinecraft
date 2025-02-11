// using System;
// using System.Numerics;
// using AvaMc.Util;
// using AvaMc.WorldBuilds;
//
// namespace AvaMc.Coordinates;
//
// public readonly struct ChunkOffset : IEquatable<ChunkOffset>
// {
//     Vector3I Value { get; }
//
//     public override int GetHashCode() => Value.GetHashCode();
//
//     public ChunkOffset(Vector3I value)
//     {
//         Value = value;
//     }
//
//     public Vector3I ToChunkPosition()
//     {
//         return Vector3I.Multiply(Value, Chunk.ChunkSize);
//     }
//
//     public ChunkOffset ToNeighbor(Direction direction)
//     {
//         var val = Value + direction.Vector3I;
//         return new(val);
//     }
//
//     public int Distance(Vector3I other)
//     {
//         return (int)Vector3I.Distance(Value, other);
//     }
//
//     public Vector3 ToNumerics()
//     {
//         return Value.ToNumerics();
//     }
//
//     public Vector3I ToInernal()
//     {
//         return Value;
//     }
//
//     public bool Equals(ChunkOffset other)
//     {
//         return Value.Equals(other.Value);
//     }
//
//     public override bool Equals(object? obj)
//     {
//         return obj is ChunkOffset other && Equals(other);
//     }
//
//     public static bool operator ==(ChunkOffset offset, Vector3I other)
//     {
//         return offset.Value == other;
//     }
//
//     public static bool operator !=(ChunkOffset offset, Vector3I other)
//     {
//         return offset.Value != other;
//     }
// }
