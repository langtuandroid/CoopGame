using TriangleNet.Geometry;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Chunk
{
public interface IChunk
{
    public void AddChunkNavMesh(
        Vector2 position,
        float chunkSize,
        Polygon polygon
    );
}
}