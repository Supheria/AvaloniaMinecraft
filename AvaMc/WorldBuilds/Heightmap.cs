using Microsoft.Xna.Framework;

namespace AvaMc.WorldBuilds;

public class Heightmap
{
    // X-Z offset
    public Vector2 Offset { get; set; }
    int[,] Data { get; set; }

    WorldgenData WorldgenData { get; set; }
    bool Generated { get; set; } = true;

    public long GetData(float x, float z)
    {
        return Data[(int)x, (int)z];
    }

    public void SetData(float x, float z, float value)
    {
        Data[(int)x, (int)z] = (int)value;
    }
}
