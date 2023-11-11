using System;

namespace Main.Scripts.LevelGeneration.Chunk
{
[Flags]
public enum ChunkConnectionType
{
    TopSide = 1,
    RightTopCorner = 1 << 2,
    RightSide = 1 << 3,
    RightBottomCorner = 1 << 4,
    BottomSide = 1 << 5,
    LeftBottomCorner = 1 << 6,
    LeftSide = 1 << 7,
    LeftTopCorner = 1 << 8,
}
}