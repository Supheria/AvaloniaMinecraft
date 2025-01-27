using Microsoft.Xna.Framework;

namespace AvaMc.Util;

public readonly record struct CameraMatrices(Matrix4 Project, Matrix4 View);
