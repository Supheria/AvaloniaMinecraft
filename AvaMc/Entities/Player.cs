using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Avalonia.Input;
using AvaMc.Blocks;
using AvaMc.Coordinates;
using AvaMc.Extensions;
using AvaMc.Gfx;
using AvaMc.Util;
using AvaMc.WorldBuilds;
using Silk.NET.OpenGLES;

namespace AvaMc.Entities;

// TODO
public sealed class Player
{
    const float MouseSensitivity = 3.2f;
    const float Speed = 0.22f;

    // const float Speed = 0.05f;
    World World { get; set; }
    public PerspectiveCamera Camera { get; set; }
    bool HasLookBlock { get; set; }
    BlockPosition LookBlock { get; set; }
    Direction LookFace { get; set; }
    public Vector3I ChunkOffset { get; set; }
    Vector3I BlockPosition { get; set; }
    public bool ChunkOffsetChanged { get; set; }
    public bool BlockPositionChanged { get; set; }
    BlockId SelectedBlockId { get; set; } = BlockId.Air;
    Dictionary<Key, BlockId> BlockPack { get; } =
        new()
        {
            [Key.D1] = BlockId.Torch,
            [Key.D2] = BlockId.Stone,
            [Key.D3] = BlockId.Planks,
            [Key.D4] = BlockId.Sand,
            [Key.D5] = BlockId.Glass,
            [Key.D6] = BlockId.Water,
            [Key.D7] = BlockId.Log,
            [Key.D8] = BlockId.Leaves,
            [Key.D9] = BlockId.Rose,
            [Key.D0] = BlockId.Coal,
        };

    public Player(World world)
    {
        World = world;
        Camera = new();
        Camera.Initialize(75, true);
    }

    public void Delete(GL gl) { }

    public void Render(GL gl)
    {
        // TODO: not good here
        Camera = GlobalState.Renderer.PerspectiveCamera;
    }

    public void Update()
    {
        Camera.Update();
        // Debug.WriteLine(Camera);
        if (GlobalState.Game.Pointer[PointerButton.Left].Down)
        {
            Camera.Pitch -=
                GlobalState.Game.Pointer.Delta.Y / (GlobalState.Game.FrameDelta / (MouseSensitivity * 10000));
            Camera.Yaw -=
                GlobalState.Game.Pointer.Delta.X / (GlobalState.Game.FrameDelta / (MouseSensitivity * 10000));
        }

        var blockWorldPosition = new BlockPosition(Camera.Position);
        var blockPosition = blockWorldPosition.IntoChunk();
        BlockPositionChanged = BlockPosition != blockPosition;
        if (BlockPositionChanged)
            BlockPosition = blockPosition;

        var chunkOffset = blockWorldPosition.ToChunkOffset();
        ChunkOffsetChanged = ChunkOffset != chunkOffset;
        if (ChunkOffsetChanged)
            ChunkOffset = chunkOffset;

        foreach (var (key, id) in BlockPack)
        {
            if (GlobalState.Game.Keyboard[key].Down)
                SelectedBlockId = id;
        }
    }

    public void Tick()
    {
        var forward = new Vector3(float.Sin(Camera.Yaw), 0, float.Cos(Camera.Yaw));
        var right = Vector3.Cross(Vector3.UnitY, forward);

        var direction = Vector3.Zero;
        var keyboard = GlobalState.Game.Keyboard;
        if (keyboard[Key.W].Down)
            direction = Vector3.Add(direction, forward);
        if (keyboard[Key.S].Down)
            direction = Vector3.Subtract(direction, forward);
        if (keyboard[Key.A].Down)
            direction = Vector3.Add(direction, right);
        if (keyboard[Key.D].Down)
            direction = Vector3.Subtract(direction, right);
        if (keyboard[Key.Space].Down)
            direction = Vector3.Add(direction, Vector3.UnitY);
        if (keyboard[Key.LeftShift].Down)
            direction = Vector3.Subtract(direction, Vector3.UnitY);
        direction = Vector3.Normalize(direction);
        Vector3 movement;
        if (direction.IsNaN())
            movement = Vector3.Zero;
        else
            movement = Vector3.Multiply(direction, Speed);
        Camera.Position = Vector3.Add(Camera.Position, movement);

        const float reach = 6f;
        var ray = new Ray(Camera.Position, Camera.Direction);
        // var ray = GetRay();
        HasLookBlock = ray.RayBlock(reach, out var lookBlock, out var lookFace);
        if (HasLookBlock && lookFace != null)
        {
            LookBlock = lookBlock;
            LookFace = lookFace;
            if (GlobalState.Game.Pointer[PointerButton.Left].PressedTick)
                World.SetBlockId(LookBlock, BlockId.Air);
            if (GlobalState.Game.Pointer[PointerButton.Right].PressedTick)
            {
                var pos = LookBlock.ToNeighbor(LookFace);
                World.SetBlockId(pos, SelectedBlockId);
            }
        }
    }

    private Ray GetRay()
    {
        var size = GlobalState.Game.WindowSize;
        var width = (float)size.Width;
        var height = (float)size.Height;
        // heavily influenced by: http://antongerdelan.net/opengl/raycasting.html
        // viewport coordinate system
        // normalized device coordinates
        var point = GlobalState.Game.Pointer.Position;
        var x = (2f * point.X) / width - 1f;
        var y = 1f - (2f * point.Y) / height;
        var z = 1f;
        var rayNormalizedDeviceCoordinates = new Vector3(x, y, z);

        // 4D homogeneous clip coordinates
        var rayClip = new Vector4(
            rayNormalizedDeviceCoordinates.X,
            rayNormalizedDeviceCoordinates.Y,
            -1f,
            1f
        );

        // 4D eye (camera) coordinates
        var projection = Camera.Project;
        Matrix4x4.Invert(projection, out var projectionInvert);
        var rayEye = Vector4.Transform(rayClip, projectionInvert);
        rayEye = new Vector4(rayEye.X, rayEye.Y, -1f, 0f);

        // 4D world coordinates
        var view = Camera.View;
        Matrix4x4.Invert(view, out var viewInvert);
        var r = Vector4.Transform(rayEye, viewInvert);
        var direction = Vector3.Normalize(new(r.X, r.Y, r.Z));
        return new(Camera.Position, direction);
    }
}
