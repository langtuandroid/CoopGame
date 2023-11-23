using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.LevelGeneration.Chunk;
using Main.Scripts.LevelGeneration.Configs;
using Main.Scripts.LevelGeneration.Places;
using Main.Scripts.LevelGeneration.Places.Crossroads;
using Main.Scripts.LevelGeneration.Places.EscortFinish;
using Main.Scripts.LevelGeneration.Places.Outside;
using Main.Scripts.LevelGeneration.Places.Road;
using Main.Scripts.LevelGeneration.Places.Spawn;
using UnityEngine;

namespace Main.Scripts.LevelGeneration
{
public class LevelGenerator
{
    public IChunk[][] Generate(
        int seed,
        LevelGenerationConfig levelGenerationConfig,
        DecorationsPack decorationsPack
    )
    {
        var random = new NetworkRNG(seed);

        var mapData = levelGenerationConfig switch
        {
            EscortLevelGenerationConfig escortLevelGenerationConfig => GenerateEscortMapData(ref random, escortLevelGenerationConfig),
            _ => throw new ArgumentOutOfRangeException(nameof(levelGenerationConfig), levelGenerationConfig, null)
        };
        
        var map = GenerateChunks(
            mapData: mapData,
            ref random,
            chunkSize: levelGenerationConfig.ChunkSize,
            minRoadWidth: levelGenerationConfig.MinRoadWidth,
            maxRoadWidth: levelGenerationConfig.MaxRoadWidth,
            outlineOffset: levelGenerationConfig.OutlineOffset,
            decorationsPack: decorationsPack
        );

        return map;
    }

    private MapData GenerateEscortMapData(
        ref NetworkRNG random,
        EscortLevelGenerationConfig escortConfig
    )
    {
        var placesList = new List<Place>();
        var roadsList = new List<KeyValuePair<int, int>>();

        var spawnPlace = new SpawnPlace(
            position: new Vector2Int(0, 0),
            gateDirection: GateDirection.Right
        );

        placesList.Add(spawnPlace);

        var dividerStep = escortConfig.DividersStep;
        var maxOffset = escortConfig.NextPointMaxOffset;
        var availableRoadLength = (double)escortConfig.RoadLength;

        var maxStepLength = Math.Sqrt(maxOffset * maxOffset + dividerStep * dividerStep);


        while (availableRoadLength >= 1)
        {
            var lastPlacePosition = placesList[^1].Position;
            Place place;
            if (availableRoadLength >= maxStepLength)
            {
                var offset = random.RangeInclusive(-maxOffset, maxOffset);
                place = new CrossroadsPlace(
                    position: new Vector2Int(lastPlacePosition.x + dividerStep, lastPlacePosition.y + offset),
                    radius: 3 //todo
                );
                availableRoadLength -= Math.Sqrt(dividerStep * dividerStep + offset * offset);
            }
            else
            {
                var availableOffset = Math.Min(maxOffset, (int)(maxOffset * availableRoadLength / maxStepLength));
                var offset = random.RangeInclusive(-availableOffset, availableOffset);
                place = new EscortFinishPlace(
                    position: new Vector2Int(
                        lastPlacePosition.x +
                        (int)Math.Sqrt(availableRoadLength * availableRoadLength - offset * offset),
                        lastPlacePosition.y + offset
                    ),
                    gateDirection: GateDirection.Left
                );
                availableRoadLength = 0;
            }

            placesList.Add(place);
            roadsList.Add(new KeyValuePair<int, int>(placesList.Count - 2, placesList.Count - 1));
            if (placesList[^2] is CrossroadsPlace crossroadsPlaceFrom)
            {
                crossroadsPlaceFrom.AddRoadToPlace(placesList[^1]);
            }

            if (placesList[^1] is CrossroadsPlace crossroadsPlaceTo)
            {
                crossroadsPlaceTo.AddRoadToPlace(placesList[^2]);
            }

            Debug.Log($"Point {placesList.Count}: {place.Position}");
        }

        return new MapData
        {
            Places = placesList,
            Roads = roadsList
        };
    }

    private IChunk[][] GenerateChunks(
        MapData mapData,
        ref NetworkRNG random,
        int chunkSize,
        int minRoadWidth,
        int maxRoadWidth,
        int outlineOffset,
        DecorationsPack decorationsPack
    )
    {
        var places = mapData.Places;

        var minX = 0;
        var maxX = 0;
        var minY = 0;
        var maxY = 0;

        for (var i = 0; i < places.Count; i++)
        {
            places[i].GetBounds(
                out var minXBounds,
                out var maxXBounds,
                out var minYBounds,
                out var maxYBounds
            );

            minX = Math.Min(minX, minXBounds);
            maxX = Math.Max(maxX, maxXBounds);
            minY = Math.Min(minY, minYBounds);
            maxY = Math.Max(maxY, maxYBounds);
        }

        var offsetX = -minX + outlineOffset;
        var offsetY = -minY + outlineOffset;

        foreach (var place in places)
        {
            place.Position = new Vector2Int(place.Position.x + offsetX, place.Position.y + offsetY);
        }

        var xChunksCount = maxX - minX
                           + 1 //Inclusive
                           + outlineOffset * 2;
        var yChunksCount = maxY - minY
                           + 1 //Inclusive
                           + outlineOffset * 2;


        var map = new IChunk?[xChunksCount][];
        for (var i = 0; i < map.Length; i++)
        {
            map[i] = new IChunk?[yChunksCount];
        }

        foreach (var place in places)
        {
            place.FillMap(map, chunkSize, ref random);
        }

        var roads = mapData.Roads;

        for (var i = 0; i < roads.Count; i++)
        {
            FillRoad(
                map,
                ref random,
                places[roads[i].Key].Position,
                places[roads[i].Value].Position,
                minRoadWidth,
                maxRoadWidth
            );
        }

        GenerateDecorations(
            map,
            ref random,
            places,
            chunkSize,
            decorationsPack
        );

        for (var i = 0; i < map.Length; i++)
        {
            for (var j = 0; j < map[i].Length; j++)
            {
                //todo сделать ленивую инициализацию, генерировать только близлежащие к рабочим тайлам для генерации навмеша
                var minHeightLevel = int.MaxValue;
                var maxHeightLevel = 2;

                if (i > 0)
                {
                    minHeightLevel = GetMinHeightLevel(minHeightLevel, map[i - 1][j]);
                }

                if (i < map.Length - 1)
                {
                    minHeightLevel = GetMinHeightLevel(minHeightLevel, map[i + 1][j]);
                }

                if (j > 0)
                {
                    minHeightLevel = GetMinHeightLevel(minHeightLevel, map[i][j - 1]);
                }

                if (j < map[i].Length - 1)
                {
                    minHeightLevel = GetMinHeightLevel(minHeightLevel, map[i][j + 1]);
                }

                if (i > 0 && j > 0)
                {
                    minHeightLevel = GetMinHeightLevel(minHeightLevel, map[i - 1][j - 1]);
                }

                if (i > 0 && j < map[i].Length - 1)
                {
                    minHeightLevel = GetMinHeightLevel(minHeightLevel, map[i - 1][j + 1]);
                }

                if (i < map.Length - 1 && j > 0)
                {
                    minHeightLevel = GetMinHeightLevel(minHeightLevel, map[i + 1][j - 1]);
                }

                if (i < map.Length - 1 && j < map[i].Length - 1)
                {
                    minHeightLevel = GetMinHeightLevel(minHeightLevel, map[i + 1][j + 1]);
                }

                if (minHeightLevel == int.MaxValue)
                {
                    minHeightLevel = 0;
                }

                var heightLevel = random.RangeInclusive(1, Math.Min(minHeightLevel + 1, maxHeightLevel));
                map[i][j] ??= new OutsideChunk(
                    heightLevel: heightLevel,
                    fillData: OutsideChunkHelper.GetChunkFillData(
                        ChunkHelper.GetChunkConnectionTypes(map, i, j, ChunkHelper.IsNotOutside))
                );
            }
        }

        return map!;
    }

    private int GetMinHeightLevel(int minHeightLevel, IChunk? chunk)
    {
        if (chunk != null)
        {
            return Math.Min(
                minHeightLevel,
                chunk is OutsideChunk topOutsideChunk ? topOutsideChunk.HeightLevel : 0
            );
        }

        return minHeightLevel;
    }

    private void FillRoad(
        IChunk?[][] map,
        ref NetworkRNG random,
        Vector2Int pointA,
        Vector2Int pointB,
        int minRoadWidth,
        int maxRoadWidth
    )
    {
        Vector2Int fromPoint;
        Vector2Int toPoint;

        if (pointA.x < pointB.x)
        {
            fromPoint = pointA;
            toPoint = pointB;
        }
        else
        {
            fromPoint = pointB;
            toPoint = pointA;
        }

        var deltaX = Math.Abs(toPoint.x - fromPoint.x);
        var deltaY = Math.Abs(toPoint.y - fromPoint.y);

        if (deltaY > deltaX)
        {
            var stepCoordX = (toPoint.x - fromPoint.x) / (float)deltaY;
            var stepY = Math.Sign(toPoint.y - fromPoint.y);
            for (var i = 0; i <= deltaY; i++)
            {
                var coordX = fromPoint.x + i * stepCoordX;
                var x = (int)(coordX + 0.5);
                var y = fromPoint.y + i * stepY;

                map[x][y] ??= new RoadChunk(fromPoint, toPoint);

                var roadWidth = random.RangeInclusive(minRoadWidth, maxRoadWidth);
                for (var k = 1; k <= roadWidth / 2; k++)
                {
                    if (x - k > 0 && map[x - k][y] == null)
                    {
                        map[x - k][y] = new RoadChunk(fromPoint, toPoint);
                    }
                }

                roadWidth = random.RangeInclusive(minRoadWidth, maxRoadWidth);
                for (var k = 1; k <= roadWidth / 2; k++)
                {
                    if (x + k < map.Length - 1 && map[x + k][y] == null)
                    {
                        map[x + k][y] = new RoadChunk(fromPoint, toPoint);
                    }
                }
            }
        }
        else
        {
            var stepCoordY = (toPoint.y - fromPoint.y) / (float)deltaX;
            var stepX = Math.Sign(toPoint.x - fromPoint.x);
            for (var i = 0; i <= deltaX; i++)
            {
                var coordY = fromPoint.y + i * stepCoordY;
                var y = (int)(coordY + 0.5);
                var x = fromPoint.x + i * stepX;

                map[x][y] ??= new RoadChunk(fromPoint, toPoint);

                var roadWidth = random.RangeInclusive(minRoadWidth, maxRoadWidth);
                for (var k = 1; k <= roadWidth / 2; k++)
                {
                    if (y - k >= 0 && map[x][y - k] == null)
                    {
                        map[x][y - k] = new RoadChunk(fromPoint, toPoint);
                    }
                }

                roadWidth = random.RangeInclusive(minRoadWidth, maxRoadWidth);
                for (var k = 1; k <= roadWidth / 2; k++)
                {
                    if (y + k < map[x].Length && map[x][y + k] == null)
                    {
                        map[x][y + k] = new RoadChunk(fromPoint, toPoint);
                    }
                }
            }
        }
    }

    private void GenerateDecorations(
        IChunk?[][] map,
        ref NetworkRNG random,
        List<Place> places,
        int chunkSize,
        DecorationsPack decorationsPack
    )
    {
        foreach (var place in places)
        {
            place.FillDecorations(map, chunkSize, decorationsPack, ref random);
        }
    }
}
}