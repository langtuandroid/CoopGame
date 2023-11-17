using System;
using Main.Scripts.LevelGeneration.Places.Outside;

namespace Main.Scripts.LevelGeneration.Chunk
{
public static class ChunkHelper
{
    public static ChunkConnectionType GetChunkConnectionTypes(
        IChunk?[][] map,
        int x,
        int y,
        Func<IChunk?, bool> IsHasConnectionFunc
    )
    {
        var xChunksCount = map.Length;
        var yChunksCount = map[x].Length;

        ChunkConnectionType connectionTypes = 0;
        if (x > 0 && IsHasConnectionFunc(map[x - 1][y]))
        {
            connectionTypes |= ChunkConnectionType.LeftSide;
        }

        if (x < xChunksCount - 1 && IsHasConnectionFunc(map[x + 1][y]))
        {
            connectionTypes |= ChunkConnectionType.RightSide;
        }

        if (y > 0 && IsHasConnectionFunc(map[x][y - 1]))
        {
            connectionTypes |= ChunkConnectionType.BottomSide;
        }

        if (y < yChunksCount - 1 && IsHasConnectionFunc(map[x][y + 1]))
        {
            connectionTypes |= ChunkConnectionType.TopSide;
        }

        if (x > 0 && y > 0 && IsHasConnectionFunc(map[x - 1][y - 1]))
        {
            connectionTypes |= ChunkConnectionType.LeftBottomCorner;
        }

        if (x < xChunksCount - 1 && y > 0 && IsHasConnectionFunc(map[x + 1][y - 1]))
        {
            connectionTypes |= ChunkConnectionType.RightBottomCorner;
        }

        if (x > 0 && y < yChunksCount - 1 && IsHasConnectionFunc(map[x - 1][y + 1]))
        {
            connectionTypes |= ChunkConnectionType.LeftTopCorner;
        }

        if (x < xChunksCount - 1 && y < yChunksCount - 1 && IsHasConnectionFunc(map[x + 1][y + 1]))
        {
            connectionTypes |= ChunkConnectionType.RightTopCorner;
        }

        return connectionTypes;
    }

    public static bool IsNotOutside(IChunk? chunk)
    {
        return chunk != null && chunk is not OutsideChunk;
    }

    public static bool IsHigherHeightLevel(IChunk? chunk, int heightLevel)
    {
        return chunk is not OutsideChunk outsideChunk || outsideChunk.HeightLevel > heightLevel;
    }

    public static bool IsLowerHeightLevel(IChunk? chunk, int heightLevel)
    {
        return chunk is not OutsideChunk outsideChunk || outsideChunk.HeightLevel < heightLevel;
    }
}
}