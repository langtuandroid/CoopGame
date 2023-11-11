using Main.Scripts.LevelGeneration.Chunk;
using TriangleNet;
using TriangleNet.Geometry;
using UnityEngine;
using UnityEngine.Pool;

namespace Main.Scripts.LevelGeneration.Places.Road
{
public class RoadChunk : IChunk
{
    public Vector2Int FromPoint { get; }
    public Vector2Int ToPoint { get; }

    public RoadChunk(Vector2Int fromPoint, Vector2Int toPoint)
    {
        FromPoint = fromPoint;
        ToPoint = toPoint;
    }
    
    public void AddChunkNavMesh(Vector2 position, float chunkSize, Polygon polygon)
    {
        var pointsList = ListPool<Vector2>.Get();
        
        pointsList.Add(new Vector2(position.x, position.y));
        pointsList.Add(new Vector2(position.x, position.y + chunkSize));
        pointsList.Add(new Vector2(position.x + chunkSize, position.y + chunkSize));
        pointsList.Add(new Vector2(position.x + chunkSize, position.y));
        
        polygon.Add(pointsList);
        
        pointsList.Clear();
        ListPool<Vector2>.Release(pointsList);
    }
}
}