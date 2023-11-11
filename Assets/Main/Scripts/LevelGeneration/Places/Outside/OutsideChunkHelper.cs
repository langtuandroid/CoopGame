using Main.Scripts.LevelGeneration.Chunk;

namespace Main.Scripts.LevelGeneration.Places.Outside
{
public static class OutsideChunkHelper
{
    public static OutsideChunkFillData GetChunkFillData(ChunkConnectionType connectionTypes)
    {
        var hasTopSide = connectionTypes.HasFlag(ChunkConnectionType.TopSide);
        var hasRightSide = connectionTypes.HasFlag(ChunkConnectionType.RightSide);
        var hasBottomSide = connectionTypes.HasFlag(ChunkConnectionType.BottomSide);
        var hasLeftSide = connectionTypes.HasFlag(ChunkConnectionType.LeftSide);
        var hasRightTopCorner = connectionTypes.HasFlag(ChunkConnectionType.RightTopCorner);
        var hasRightBottomCorner = connectionTypes.HasFlag(ChunkConnectionType.RightBottomCorner);
        var hasLeftBottomCorner = connectionTypes.HasFlag(ChunkConnectionType.LeftBottomCorner);
        var hasLeftTopCorner = connectionTypes.HasFlag(ChunkConnectionType.LeftTopCorner);

        var centerType = ChunkCenterType.None;
        if (ShouldSpawnCenterInner(hasTopSide,
                hasLeftSide,
                hasRightSide,
                hasBottomSide,
                hasRightBottomCorner))
        {
            centerType = ChunkCenterType.LeftTop;
        } else if (ShouldSpawnCenterInner(hasRightSide,
                       hasTopSide,
                       hasBottomSide,
                       hasLeftSide,
                       hasLeftBottomCorner))
        {
            centerType = ChunkCenterType.RightTop;
        } else if (ShouldSpawnCenterInner(hasBottomSide,
                       hasRightSide,
                       hasLeftSide,
                       hasTopSide,
                       hasLeftTopCorner))
        {
            centerType = ChunkCenterType.RightBottom;
        } else if (ShouldSpawnCenterInner(hasLeftSide,
                       hasBottomSide,
                       hasTopSide,
                       hasRightSide,
                       hasRightTopCorner))
        {
            centerType = ChunkCenterType.LeftBottom;
        }

        var leftTopCornerType = ChunkCornerType.None;
        if (centerType is ChunkCenterType.None or ChunkCenterType.RightBottom)
        {
            leftTopCornerType = GetCornerType(hasTopSide,
                hasLeftSide,
                hasLeftTopCorner
            );
        }

        var rightTopCornerType = ChunkCornerType.None;
        if (centerType is ChunkCenterType.None or ChunkCenterType.LeftBottom)
        {
            rightTopCornerType = GetCornerType(
                hasRightSide,
                hasTopSide,
                hasRightTopCorner
            );
        }

        var rightBottomCornerType = ChunkCornerType.None;
        if (centerType is ChunkCenterType.None or ChunkCenterType.LeftTop)
        {
            rightBottomCornerType = GetCornerType(
                hasBottomSide,
                hasRightSide,
                hasRightBottomCorner
            );
        }

        var leftBottomCornerType = ChunkCornerType.None;
        if (centerType is ChunkCenterType.None or ChunkCenterType.RightTop)
        {
            leftBottomCornerType = GetCornerType(
                hasLeftSide,
                hasBottomSide,
                hasLeftBottomCorner
            );
        }

        return new OutsideChunkFillData
        {
            LeftTopCornerType = leftTopCornerType,
            RightTopCornerType = rightTopCornerType,
            RightBottomCornerType = rightBottomCornerType,
            LeftBottomCornerType = leftBottomCornerType,
            TopSide = hasTopSide,
            RightSide = hasRightSide,
            BottomSide = hasBottomSide,
            LeftSide = hasLeftSide,
            centerType = centerType
        };
    }

    private static bool ShouldSpawnCenterInner(
        bool hasBackLeft,
        bool hasBackRight,
        bool hasForwardLeft,
        bool hasForwardRight,
        bool hasForwardCorner)
    {
        return hasBackLeft && hasBackRight && !hasForwardLeft && !hasForwardRight && !hasForwardCorner;
    }

    private static ChunkCornerType GetCornerType(
        bool hasBackLeft,
        bool hasBackRight,
        bool hasBackCorner
    )
    {
        if (hasBackLeft && hasBackRight)
        {
            return ChunkCornerType.Inner;
        }

        if (hasBackLeft)
        {
            return ChunkCornerType.LeftFlat;
        }

        if (hasBackRight)
        {
            return ChunkCornerType.RightFlat;
        }

        if (hasBackCorner)
        {
            return ChunkCornerType.Outer;
        }

        return ChunkCornerType.None;
    }
}
}