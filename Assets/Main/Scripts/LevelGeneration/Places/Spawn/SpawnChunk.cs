using System;
using Main.Scripts.LevelGeneration.Chunk;
using Main.Scripts.LevelGeneration.Configs;
using TriangleNet;
using TriangleNet.Geometry;
using UnityEngine;
using UnityEngine.Pool;

namespace Main.Scripts.LevelGeneration.Places.Spawn
{
public class SpawnChunk : IChunk
{
    public SpawnPlace SpawnPlace { get; }

    public SpawnChunk(SpawnPlace spawnPlace)
    {
        SpawnPlace = spawnPlace;
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
        throw new NotImplementedException();
    }
}
}