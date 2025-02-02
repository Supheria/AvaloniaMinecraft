namespace AvaMc.WorldBuilds;

public sealed class Threshold
{
    int Count { get; set; }
    int Max { get; }

    public Threshold(int max)
    {
        Max = max;
    }

    public bool UnderThreshold()
    {
        return Count < Max;
    }

    public void AddOne()
    {
        Count++;
    }

    public void Reset()
    {
        Count = 0;
    }
}
