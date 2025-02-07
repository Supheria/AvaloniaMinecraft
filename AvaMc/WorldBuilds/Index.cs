namespace AvaMc.WorldBuilds;

public readonly struct Index
{
    public uint Offset { get; }
    public uint Count { get; }

    public Index(uint offset, uint count)
    {
        Offset = offset;
        Count = count;
    }
}
