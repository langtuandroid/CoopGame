using Main.Scripts.LevelGeneration.Chunk;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Places
{
public abstract class Place
{
    public Vector2Int Position { get; set; }

    protected Place(Vector2Int position)
    {
        Position = position;
    }

    public abstract void GetBounds(
        out int minX,
        out int maxX,
        out int minY,
        out int maxY
    );

    public abstract void FillMap(
        IChunk?[][] map
    );
}
}