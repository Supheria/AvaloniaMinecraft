using AvaMc.Blocks;
using AvaMc.Extensions;
using AvaMc.Gfx;

namespace AvaMc.WorldBuilds;

// - 28 bits metadata/extra
// - 4 bits sunlight intensity
// - 4 bits R light
// - 4 bits G light
// - 4 bits B light
// - 4 bits light intensity
// - 16 bits block id
public struct BlockData
{
    const ulong BlockIdMask = 0x000000000000FFFF;
    const int BlockIdOffset = 0;
    const ulong TorchLightMask = 0x000000000FFFF0000;
    const int TorchLightOffset = 16;
    const ulong SunlightMask = 0x0000000F00000000;
    const int SunlightOffset = 32;
    const ulong AllLightMask = 0x0000000FFFFF0000;
    const int AllLightOffset = 16;
    const ulong MetaDataMask = 0xFFFFFF7000000000;
    const int MetaDataOffset = 36;
    ulong Data { get; set; }
    public BlockId BlockId
    {
        get => (BlockId)GetValue(BlockIdMask, BlockIdOffset);
        set => SetValue(BlockIdMask, BlockIdOffset, (int)value);
    }

    public TorchLight TorchLight
    {
        get => (TorchLight)GetValue(TorchLightMask, TorchLightOffset);
        set => SetValue(TorchLightMask, TorchLightOffset, value);
    }

    public int Sunlight
    {
        get => GetValue(SunlightMask, SunlightOffset);
        set => SetValue(SunlightMask, SunlightOffset, value);
    }

    public AllLight AllLight
    {
        get => (AllLight)GetValue(AllLightMask, AllLightOffset);
        set => SetValue(AllLightMask, AllLightOffset, value);
    }

    public BlockData()
    {
        Data = 0;
    }

    // TODO: meta data

    private int GetValue(ulong mask, int offset)
    {
        return (int)((Data & mask) >> offset);
    }

    private void SetValue(ulong mask, int offset, int value)
    {
        Data = (Data & ~mask) | (((ulong)value << offset) & mask);
    }
}
