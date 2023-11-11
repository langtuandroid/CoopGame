using Main.Scripts.LevelGeneration.Places.Outside;

namespace Main.Scripts.LevelGeneration.Chunk
{
public static class ChunkHelper
{
    public static ChunkConnectionType GetChunkConnectionTypes(
        IChunk[][] map,
        int x,
        int y
    )
    {
        var xChunksCount = map.Length;
        var yChunksCount = map[x].Length;

        ChunkConnectionType connectionTypes = 0;
        if (x > 0 && IsNotOutside(map, x - 1, y))
        {
            connectionTypes |= ChunkConnectionType.LeftSide;
        }

        if (x < xChunksCount - 1 && IsNotOutside(map, x + 1, y))
        {
            connectionTypes |= ChunkConnectionType.RightSide;
        }

        if (y > 0 && IsNotOutside(map, x, y - 1))
        {
            connectionTypes |= ChunkConnectionType.BottomSide;
        }

        if (y < yChunksCount - 1 &&IsNotOutside(map, x, y + 1))
        {
            connectionTypes |= ChunkConnectionType.TopSide;
        }

        if (x > 0 && y > 0 && IsNotOutside(map, x - 1, y - 1))
        {
            connectionTypes |= ChunkConnectionType.LeftBottomCorner;
        }

        if (x < xChunksCount - 1 && y > 0 && IsNotOutside(map, x + 1, y - 1))
        {
            connectionTypes |= ChunkConnectionType.RightBottomCorner;
        }

        if (x > 0 && y < yChunksCount - 1 && IsNotOutside(map, x - 1, y + 1))
        {
            connectionTypes |= ChunkConnectionType.LeftTopCorner;
        }

        if (x < xChunksCount - 1 && y < yChunksCount - 1 && IsNotOutside(map, x + 1, y + 1))
        {
            connectionTypes |= ChunkConnectionType.RightTopCorner;
        }

        return connectionTypes;
    }

    private static bool IsNotOutside(IChunk[][] map, int x, int y)
    {
        return map[x][y] != null && map[x][y] is not OutsideChunk;
    }
}
}