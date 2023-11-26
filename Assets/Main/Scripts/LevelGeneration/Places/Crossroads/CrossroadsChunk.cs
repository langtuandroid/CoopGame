using System;
using System.Collections.Generic;
using Main.Scripts.LevelGeneration.Chunk;
using Main.Scripts.LevelGeneration.Configs;
using Main.Scripts.LevelGeneration.Data.Colliders;
using TriangleNet;
using TriangleNet.Geometry;
using UnityEngine;
using UnityEngine.Pool;

namespace Main.Scripts.LevelGeneration.Places.Crossroads
{
public class CrossroadsChunk : IChunk
{
    public CrossroadsPlace CrossroadsPlace { get; }

    private DecorationConfig? decorationConfig;

    private Vector2 decorationPosition;

    private bool isOccupiedByDecoration;

    public CrossroadsChunk(
        CrossroadsPlace place
    )
    {
        CrossroadsPlace = place;
    }

    public void AddDecoration(DecorationConfig decorationConfig, Vector2 decorationPosition)
    {
        this.decorationConfig = decorationConfig;
        this.decorationPosition = decorationPosition;
    }

    public Vector2 GetDecorationPosition()
    {
        return decorationPosition;
    }

    public void SetOccupiedByDecoration(bool isOccupied)
    {
        isOccupiedByDecoration = isOccupied;
    }

    public DecorationConfig? GetDecorationConfig()
    {
        return decorationConfig;
    }

    public bool CanAddDecoration()
    {
        return !isOccupiedByDecoration && decorationConfig == null;
    }

    public void AddChunkNavMesh(
        Vector2 position,
        float chunkSize,
        Polygon polygon
    )
    {
        var pointsList = ListPool<Vector2>.Get();

        pointsList.Add(new Vector2(position.x, position.y));
        pointsList.Add(new Vector2(position.x, position.y + chunkSize));
        pointsList.Add(new Vector2(position.x + chunkSize, position.y + chunkSize));
        pointsList.Add(new Vector2(position.x + chunkSize, position.y));

        polygon.Add(pointsList);
        pointsList.Clear();


        if (decorationConfig != null)
        {
            var bounds = decorationConfig.ColliderInfo.Size;
            var halfBoundsX = bounds.x / 2f;
            var halfBoundsY = bounds.y / 2f;

            var centerOffset = chunkSize / 2f;

            var centerPosition = new Vector2(
                position.x + decorationPosition.x + centerOffset,
                position.y + decorationPosition.y + centerOffset
            );

            AddHole(
                leftX: Math.Max(centerPosition.x - halfBoundsX, position.x),
                rightX: Math.Min(centerPosition.x + halfBoundsX, position.x + chunkSize),
                bottomY: Math.Max(centerPosition.y - halfBoundsY, position.y),
                topY: Math.Min(centerPosition.y + halfBoundsY, position.y + chunkSize),
                polygon: polygon,
                pointsList: pointsList
            );

            if (centerPosition.x - halfBoundsX < position.x)
            {
                AddHole(
                    leftX: centerPosition.x - halfBoundsX,
                    rightX: position.x,
                    bottomY: Math.Max(centerPosition.y - halfBoundsY, position.y),
                    topY: Math.Min(centerPosition.y + halfBoundsY, position.y + chunkSize),
                    polygon: polygon,
                    pointsList: pointsList
                );
            }

            if (centerPosition.x + halfBoundsX > position.x + chunkSize)
            {
                AddHole(
                    leftX: position.x + chunkSize,
                    rightX: centerPosition.x + halfBoundsX,
                    bottomY: Math.Max(centerPosition.y - halfBoundsY, position.y),
                    topY: Math.Min(centerPosition.y + halfBoundsY, position.y + chunkSize),
                    polygon: polygon,
                    pointsList: pointsList
                );
            }

            if (centerPosition.y - halfBoundsY < position.y)
            {
                AddHole(
                    leftX: Math.Max(centerPosition.x - halfBoundsX, position.x),
                    rightX: Math.Min(centerPosition.x + halfBoundsX, position.x + chunkSize),
                    bottomY: centerPosition.y - halfBoundsY,
                    topY: position.y,
                    polygon: polygon,
                    pointsList: pointsList
                );
            }

            if (centerPosition.y + halfBoundsY > position.y + chunkSize)
            {
                AddHole(
                    leftX: Math.Max(centerPosition.x - halfBoundsX, position.x),
                    rightX: Math.Min(centerPosition.x + halfBoundsX, position.x + chunkSize),
                    bottomY: position.y + chunkSize,
                    topY: centerPosition.y + halfBoundsY,
                    polygon: polygon,
                    pointsList: pointsList
                );
            }

            if (centerPosition.x - halfBoundsX < position.x && centerPosition.y - halfBoundsY < position.y)
            {
                AddHole(
                    leftX: centerPosition.x - halfBoundsX,
                    rightX: position.x,
                    bottomY: centerPosition.y - halfBoundsY,
                    topY: position.y,
                    polygon: polygon,
                    pointsList: pointsList
                );
            }

            if (centerPosition.x - halfBoundsX < position.x && centerPosition.y + halfBoundsY > position.y + chunkSize)
            {
                AddHole(
                    leftX: centerPosition.x - halfBoundsX,
                    rightX: position.x,
                    bottomY: position.y + chunkSize,
                    topY: centerPosition.y + halfBoundsY,
                    polygon: polygon,
                    pointsList: pointsList
                );
            }

            if (centerPosition.x + halfBoundsX > position.x + chunkSize && centerPosition.y - halfBoundsY < position.y)
            {
                AddHole(
                    leftX: position.x + chunkSize,
                    rightX: centerPosition.x + halfBoundsX,
                    bottomY: centerPosition.y - halfBoundsY,
                    topY: position.y,
                    polygon: polygon,
                    pointsList: pointsList
                );
            }

            if (centerPosition.x + halfBoundsX > position.x + chunkSize &&
                centerPosition.y + halfBoundsY > position.y + chunkSize)
            {
                AddHole(
                    leftX: position.x + chunkSize,
                    rightX: centerPosition.x + halfBoundsX,
                    bottomY: position.y + chunkSize,
                    topY: centerPosition.y + halfBoundsY,
                    polygon: polygon,
                    pointsList: pointsList
                );
            }
        }

        ListPool<Vector2>.Release(pointsList);
    }

    private void AddHole(float leftX, float rightX, float bottomY, float topY, Polygon polygon,
        List<Vector2> pointsList)
    {
        pointsList.Add(new Vector2(
            leftX,
            bottomY
        ));
        pointsList.Add(new Vector2(
            leftX,
            topY
        ));
        pointsList.Add(new Vector2(
            rightX,
            topY
        ));
        pointsList.Add(new Vector2(
            rightX,
            bottomY
        ));

        polygon.Add(pointsList, true);
        pointsList.Clear();
    }

    public void GetColliders(
        Vector2 chunkPosition,
        MapGenerationConfig mapGenerationConfig,
        List<ColliderData> colliders
    )
    {
        if (decorationConfig == null) return;

        colliders.Add(
            new ColliderData
            {
                Info = decorationConfig.ColliderInfo,
                Position = chunkPosition + decorationPosition,
                Rotation = 0
            }
        );
    }
}
}