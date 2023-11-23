using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Helpers;
using Main.Scripts.LevelGeneration.Chunk;
using Main.Scripts.LevelGeneration.Configs;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Places.Crossroads
{
public class CrossroadsPlace : Place
{
    private List<Place> roadToPlacesList = new();

    public int Radius { get; }

    public CrossroadsPlace(
        Vector2Int position,
        int radius
    ) : base(position)
    {
        Radius = radius;
    }

    public void AddRoadToPlace(Place place)
    {
        roadToPlacesList.Add(place);
    }

    public List<Place> GetRoadsToPlacesList()
    {
        return roadToPlacesList;
    }

    public override void GetBounds(out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = Position.x - Radius;
        maxX = Position.x + Radius;
        minY = Position.y - Radius;
        maxY = Position.y + Radius;
    }

    public override void FillMap(
        IChunk?[][] map,
        int chunkSize,
        ref NetworkRNG random
    )
    {
        GetBounds(
            out var minX,
            out var maxX,
            out var minY,
            out var maxY
        );

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                if (Math.Pow(Math.Abs(x - Position.x) - 0.25f, 2)
                    + Math.Pow(Math.Abs(y - Position.y) - 0.25f, 2)
                    <= Radius * Radius)
                {
                    map[x][y] ??= new CrossroadsChunk(this);
                }
            }
        }
    }

    public override void FillDecorations(IChunk?[][] map, int chunkSize, DecorationsPack decorationsPack,
        ref NetworkRNG random)
    {
        GetBounds(
            out var minX,
            out var maxX,
            out var minY,
            out var maxY
        );

        for (var decorIndex = 0; decorIndex < 3; decorIndex++)
        {
            var decorations = decorIndex switch
            {
                0 => decorationsPack.BigDecorations,
                1 => decorationsPack.MediumDecorations,
                2 => decorationsPack.SmallDecorations,
                _ => throw new Exception($"Decoration type {decorIndex} is not supported")
            };

            if (decorations.Count == 0)
            {
                continue;
            }

            DecorationConfig? nextDecorationConfig = null;
            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    if (map[x][y] is not CrossroadsChunk crossroadsChunk || !map[x][y]!.CanAddDecoration() ||
                        random.RangeInclusive(0, 1) == 0)
                    {
                        continue;
                    }

                    var connectionTypes = GetAvailableDecorationTypes(
                        map,
                        x,
                        y,
                        minX,
                        maxX,
                        minY,
                        maxY
                    );

                    var useTopChunk = connectionTypes.HasFlag(ChunkConnectionType.TopSide)
                                      && random.RangeInclusive(0, 1) == 1;
                    var useRightChunk = connectionTypes.HasFlag(ChunkConnectionType.RightSide)
                                        && (!useTopChunk || connectionTypes.HasFlag(ChunkConnectionType.RightTopCorner))
                                        && random.RangeInclusive(0, 1) == 1;
                    var useBottomChunk = connectionTypes.HasFlag(ChunkConnectionType.BottomSide)
                                         && (!useRightChunk ||
                                             connectionTypes.HasFlag(ChunkConnectionType.RightBottomCorner))
                                         && random.RangeInclusive(0, 1) == 1;
                    var useLeftChunk = connectionTypes.HasFlag(ChunkConnectionType.LeftSide)
                                       && (!useTopChunk || connectionTypes.HasFlag(ChunkConnectionType.LeftTopCorner))
                                       && (!useBottomChunk ||
                                           connectionTypes.HasFlag(ChunkConnectionType.LeftBottomCorner))
                                       && random.RangeInclusive(0, 1) == 1;

                    if (nextDecorationConfig == null)
                    {
                        nextDecorationConfig = decorations[random.RangeExclusive(0, decorations.Count)];
                    }

                    var bounds = nextDecorationConfig.Bounds;

                    var maxUnitRadius = GameConstants.MAX_UNIT_RADIUS;

                    var availableXHalfSize = (chunkSize - bounds.x) / 2f - maxUnitRadius;
                    var availableYHalfSize = (chunkSize - bounds.y) / 2f - maxUnitRadius;

                    var leftBoundsX = Math.Max(-chunkSize / 2f, -availableXHalfSize - (useLeftChunk ? chunkSize : 0));
                    var rightBoundsX = Math.Min(chunkSize / 2f, availableXHalfSize + (useRightChunk ? chunkSize : 0));
                    var topBoundsY = Math.Min(chunkSize / 2f, availableYHalfSize + (useTopChunk ? chunkSize : 0));
                    var bottomBoundsY = Math.Max(-chunkSize / 2f,
                        -availableYHalfSize - (useBottomChunk ? chunkSize : 0));

                    if (leftBoundsX > rightBoundsX || bottomBoundsY > topBoundsY)
                    {
                        continue;
                    }

                    var decorationPosition = new Vector2(
                        random.RangeInclusive(leftBoundsX, rightBoundsX),
                        random.RangeInclusive(bottomBoundsY, topBoundsY)
                    );

                    var occupiedTop = decorationPosition.y + bounds.y / 2f + maxUnitRadius / 2f > chunkSize / 2f;
                    var occupiedRight = decorationPosition.x + bounds.x / 2f + maxUnitRadius / 2f > chunkSize / 2f;
                    var occupiedBottom = decorationPosition.y - bounds.y / 2f - maxUnitRadius / 2f < -chunkSize / 2f;
                    var occupiedLeft = decorationPosition.x - bounds.x / 2f - maxUnitRadius / 2f < -chunkSize / 2f;

                    if (occupiedTop)
                    {
                        map[x][y + 1]!.SetOccupiedByDecoration(true);
                    }

                    if (occupiedRight && occupiedTop)
                    {
                        map[x + 1][y + 1]!.SetOccupiedByDecoration(true);
                    }

                    if (occupiedRight)
                    {
                        map[x + 1][y]!.SetOccupiedByDecoration(true);
                    }

                    if (occupiedRight && occupiedBottom)
                    {
                        map[x + 1][y - 1]!.SetOccupiedByDecoration(true);
                    }

                    if (occupiedBottom)
                    {
                        map[x][y - 1]!.SetOccupiedByDecoration(true);
                    }

                    if (occupiedLeft && occupiedBottom)
                    {
                        map[x - 1][y - 1]!.SetOccupiedByDecoration(true);
                    }

                    if (occupiedLeft)
                    {
                        map[x - 1][y]!.SetOccupiedByDecoration(true);
                    }

                    if (occupiedLeft && occupiedTop)
                    {
                        map[x - 1][y + 1]!.SetOccupiedByDecoration(true);
                    }

                    crossroadsChunk.AddDecoration(
                        nextDecorationConfig,
                        decorationPosition
                    );
                    nextDecorationConfig = null;
                }
            }
        }
    }

    private ChunkConnectionType GetAvailableDecorationTypes(
        IChunk?[][] map,
        int x,
        int y,
        int minX,
        int maxX,
        int minY,
        int maxY
    )
    {
        ChunkConnectionType connectionTypes = 0;

        if (map[x][y] == null || !map[x][y]!.CanAddDecoration())
        {
            return connectionTypes;
        }

        if (y < maxY && map[x][y + 1] != null && map[x][y + 1]!.CanAddDecoration())
        {
            connectionTypes |= ChunkConnectionType.TopSide;
        }

        if (x < maxX && y < maxY && map[x + 1][y + 1] != null && map[x + 1][y + 1]!.CanAddDecoration())
        {
            connectionTypes |= ChunkConnectionType.RightTopCorner;
        }

        if (x < maxX && map[x + 1][y] != null && map[x + 1][y]!.CanAddDecoration())
        {
            connectionTypes |= ChunkConnectionType.RightSide;
        }

        if (x < maxX && y > minY && map[x + 1][y - 1] != null && map[x + 1][y - 1]!.CanAddDecoration())
        {
            connectionTypes |= ChunkConnectionType.RightBottomCorner;
        }

        if (y > minY && map[x][y - 1] != null && map[x][y - 1]!.CanAddDecoration())
        {
            connectionTypes |= ChunkConnectionType.BottomSide;
        }

        if (x > minX && y > minY && map[x - 1][y - 1] != null && map[x - 1][y - 1]!.CanAddDecoration())
        {
            connectionTypes |= ChunkConnectionType.LeftBottomCorner;
        }

        if (x > minX && map[x - 1][y] != null && map[x - 1][y]!.CanAddDecoration())
        {
            connectionTypes |= ChunkConnectionType.LeftSide;
        }

        if (x > minX && y < maxY && map[x - 1][y + 1] != null && map[x - 1][y + 1]!.CanAddDecoration())
        {
            connectionTypes |= ChunkConnectionType.LeftTopCorner;
        }

        return connectionTypes;
    }
}
}