// using System.Numerics;
// using AvaMc.Blocks;
// using AvaMc.Gfx;
// using AvaMc.Util;
//
// namespace AvaMc.WorldBuilds;
//
// public sealed class BlockDataService
// {
//     BlockData _data = new();
//     public BlockData Data => _data;
//     public BlockId BlockId => _data.BlockId;
//     public TorchLight TorchLight => _data.TorchLight;
//     public int Sunlight => _data.Sunlight;
//     public AllLight AllLight => _data.AllLight;
//
//     public void SetBlockId(BlockId blockId, out BlockData prev, out BlockData changed)
//     {
//         prev = _data;
//         _data.BlockId = blockId;
//         changed = _data;
//     }
//     
//     public void SetSunlight(int sunlight, out BlockData prev, out BlockData changed)
//     {
//         prev = _data;
//         _data.Sunlight = sunlight;
//         changed = _data;
//     }
//     
//     public void SetAllLight(AllLight allLight, out BlockData prev, out BlockData changed)
//     {
//         prev = _data;
//         _data.AllLight = allLight;
//         changed = _data;
//     }
// }
