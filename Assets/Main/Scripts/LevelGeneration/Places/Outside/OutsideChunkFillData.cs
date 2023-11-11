using Main.Scripts.LevelGeneration.Chunk;

namespace Main.Scripts.LevelGeneration.Places.Outside
{
public struct OutsideChunkFillData
{
    public ChunkCornerType LeftTopCornerType;
    public ChunkCornerType RightTopCornerType;
    public ChunkCornerType RightBottomCornerType;
    public ChunkCornerType LeftBottomCornerType;

    public bool TopSide;
    public bool RightSide;
    public bool BottomSide;
    public bool LeftSide;

    public ChunkCenterType centerType;
}
}