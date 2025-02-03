using System.Diagnostics;
using System.Numerics;
using Avalonia.Input;
using AvaMc.Blocks;
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
    const float Speed = 0.24f;

    // const float Speed = 0.05f;
    World World { get; set; }
    public PerspectiveCamera Camera { get; set; }
    bool HasLookBlock { get; set; }
    Vector3I LookBlock { get; set; }
    Direction LookFace { get; set; }
    public Vector3I ChunkOffset { get; set; }
    Vector3I BlockPosition { get; set; }
    public bool ChunkOffsetChanged { get; set; }
    public bool BlockPositionChanged { get; set; }

    public Player(World world)
    {
        World = world;
        Camera = new();
        Camera.Initialize(float.DegreesToRadians(75));
    }

    public void Delete(GL gl) { }

    public void Render(GL gl) { }

    public void Update()
    {
        Camera.Update();
        if (State.Game.Pointer[PointerButton.Left].Down)
        {
            Camera.Pitch -=
                State.Game.Pointer.Delta.Y / (State.Game.FrameDelta / (MouseSensitivity * 10000));
            Camera.Yaw -=
                State.Game.Pointer.Delta.X / (State.Game.FrameDelta / (MouseSensitivity * 10000));
            State.Game.Pointer.Delta = Vector2.Zero;
        }

        var blockPosition = Camera.Position.CameraPosToBlockPos();
        BlockPositionChanged = BlockPosition != blockPosition;
        if (BlockPositionChanged)
            BlockPosition = blockPosition;

        var chunkOffset = blockPosition.WorldBlockPosToChunkOffset();
        ChunkOffsetChanged = ChunkOffset != chunkOffset;
        if (ChunkOffsetChanged)
            ChunkOffset = chunkOffset;
    }

    public void Tick()
    {
        var forward = new Vector3(float.Sin(Camera.Yaw), 0, float.Cos(Camera.Yaw));
        var right = Vector3.Cross(Vector3.UnitY, forward);

        var direction = Vector3.Zero;
        var keyboard = State.Game.Keyboard;
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
        // var ray = new Ray(Camera.Position, Camera.Direction);
        var ray = GetRay();
        HasLookBlock = ray.RayBlock(reach, out var lookBlock, out var lookFace);
        if (HasLookBlock && lookFace != null)
        {
            Debug.WriteLine($"look at {LookBlock}, face {LookFace}");
            LookBlock = lookBlock;
            LookFace = lookFace;
            if (State.Game.Pointer[PointerButton.Left].Pressed)
                World.SetBlockData(LookBlock, new BlockData() { BlockId = BlockId.Air });
            if (State.Game.Pointer[PointerButton.Right].Pressed)
            {
                var pos = Vector3I.Add(LookBlock, LookFace.Vector3I);
                World.SetBlockData(pos, new BlockData() { BlockId = BlockId.Sand });
            }
        }
    }
    
    private Ray GetRay()
    {
        var size = State.Game.WindowSize;
        var width = (float)size.Width;
        var height = (float)size.Height;
        // heavily influenced by: http://antongerdelan.net/opengl/raycasting.html
        // viewport coordinate system
        // normalized device coordinates
        var point = State.Game.Pointer.Position;
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
