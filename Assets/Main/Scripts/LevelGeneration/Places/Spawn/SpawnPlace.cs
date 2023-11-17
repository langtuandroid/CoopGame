using System;
using Fusion;
using Main.Scripts.LevelGeneration.Chunk;
using Main.Scripts.LevelGeneration.Configs;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Places.Spawn
{
public class SpawnPlace : Place
{
    public GateDirection GateDirection { get; }

    public SpawnPlace(
        Vector2Int position,
        GateDirection gateDirection
    ) : base(position)
    {
        GateDirection = gateDirection;
    }

    public override void GetBounds(
        out int minX,
        out int maxX,
        out int minY,
        out int maxY
    )
    {
        var spawnPlaceWidth = 4;
        var spawnPlaceHeight = 3;
        switch (GateDirection)
        {
            case GateDirection.Top:
                minX = Position.x - spawnPlaceHeight / 2;
                maxX = Position.x + spawnPlaceHeight / 2;
                minY = Position.y - spawnPlaceWidth + 1;
                maxY = Position.y;
                break;
            case GateDirection.Right:
                minX = Position.x - spawnPlaceWidth + 1;
                maxX = Position.x;
                minY = Position.y - spawnPlaceHeight / 2;
                maxY = Position.y + spawnPlaceHeight / 2;
                break;
            case GateDirection.Bottom:
                minX = Position.x - spawnPlaceHeight / 2;
                maxX = Position.x + spawnPlaceHeight / 2;
                minY = Position.y;
                maxY = Position.y + spawnPlaceWidth - 1;
                break;
            case GateDirection.Left:
                minX = Position.x;
                maxX = Position.x + spawnPlaceWidth - 1;
                minY = Position.y - spawnPlaceHeight / 2;
                maxY = Position.y + spawnPlaceHeight / 2;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void FillMap(
        IChunk?[][] map,
        int chunkSize,
        DecorationsPack decorationsPack,
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
                map[x][y] = new SpawnChunk(this);
            }
        }
    }
}
}