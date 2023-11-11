using System;
using Main.Scripts.LevelGeneration.Chunk;
using TriangleNet;
using TriangleNet.Geometry;
using UnityEngine;
using UnityEngine.Pool;

namespace Main.Scripts.LevelGeneration.Places.Outside
{
public class OutsideChunk : IChunk
{
    public OutsideChunkFillData FillData { get; }

    public OutsideChunk(OutsideChunkFillData fillData)
    {
        FillData = fillData;
    }

    public void AddChunkNavMesh(Vector2 position, float chunkSize, Polygon polygon)
    {
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
}
}