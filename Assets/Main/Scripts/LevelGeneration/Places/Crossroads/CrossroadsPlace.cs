using System;
using Main.Scripts.LevelGeneration.Chunk;
using UnityEngine;

namespace Main.Scripts.LevelGeneration.Places.Crossroads
{
public class CrossroadsPlace : Place
{
    public int Radius { get; }

    public CrossroadsPlace(
        Vector2Int position,
        int radius
    ) : base(position)
    {
        Radius = radius;
    }

    public override void GetBounds(out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = Position.x - Radius;
        maxX = Position.x + Radius;
        minY = Position.y - Radius;
        maxY = Position.y + Radius;
    }

    public override void FillMap(
        IChunk[][] map
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
                    map[x][y] = new CrossroadsChunk(this);
                }
            }
        }
    }
}
}