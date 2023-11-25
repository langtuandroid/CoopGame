using System;
using System.Collections.Generic;
using Main.Scripts.LevelGeneration.Chunk;
using Main.Scripts.LevelGeneration.Configs;
using Main.Scripts.LevelGeneration.Data.Colliders;
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

    public void AddDecoration(DecorationConfig decorationConfig, Vector2 Position)
    {
        throw new NotImplementedException();
    }

    public DecorationConfig? GetDecorationConfig()
    {
        throw new NotImplementedException();
    }

    public Vector2 GetDecorationPosition()
    {
        throw new NotImplementedException();
    }

    public void SetOccupiedByDecoration(bool occupied)
    {
        throw new NotImplementedException();
    }

    public bool CanAddDecoration()
    {
        return false;
    }

    public void GetColliders(
        Vector2 chunkPosition,
        LevelGenerationConfig levelGenerationConfig,
        List<ColliderData> colliders
    ) { }
}
}