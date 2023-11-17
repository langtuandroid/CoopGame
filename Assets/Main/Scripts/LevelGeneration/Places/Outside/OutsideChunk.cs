using System;
using Main.Scripts.LevelGeneration.Chunk;
using Main.Scripts.LevelGeneration.Configs;
using TriangleNet;
using TriangleNet.Geometry;
using UnityEngine;
using UnityEngine.Pool;

namespace Main.Scripts.LevelGeneration.Places.Outside
{
public class OutsideChunk : IChunk
{
    public int HeightLevel;
    public OutsideChunkFillData FillData { get; }

    public OutsideChunk(int heightLevel, OutsideChunkFillData fillData)
    {
        FillData = fillData;
        HeightLevel = heightLevel;
    }

    public void AddChunkNavMesh(Vector2 position, float chunkSize, Polygon polygon)
    {
        if (HeightLevel > 1) return;
        
        var pointsList = ListPool<Vector2>.Get();

        switch (FillData.centerType)
        {
            case ChunkCenterType.None:
                break;
            case ChunkCenterType.LeftTop:
                pointsList.Add(new Vector2(position.x, position.y + chunkSize));
                pointsList.Add(new Vector2(position.x, position.y));
                pointsList.Add(new Vector2(position.x + chunkSize, position.y + chunkSize));

                polygon.Add(pointsList);
                break;
            case ChunkCenterType.RightTop:
                pointsList.Add(new Vector2(position.x + chunkSize, position.y + chunkSize));
                pointsList.Add(new Vector2(position.x, position.y + chunkSize));
                pointsList.Add(new Vector2(position.x + chunkSize, position.y));

                polygon.Add(pointsList);
                break;
            case ChunkCenterType.RightBottom:
                pointsList.Add(new Vector2(position.x + chunkSize, position.y));
                pointsList.Add(new Vector2(position.x + chunkSize, position.y + chunkSize));
                pointsList.Add(new Vector2(position.x, position.y));

                polygon.Add(pointsList);
                break;
            case ChunkCenterType.LeftBottom:
                pointsList.Add(new Vector2(position.x, position.y));
                pointsList.Add(new Vector2(position.x + chunkSize, position.y));
                pointsList.Add(new Vector2(position.x, position.y + chunkSize));

                polygon.Add(pointsList);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

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