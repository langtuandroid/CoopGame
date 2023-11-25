using System.Collections.Generic;
using Main.Scripts.LevelGeneration.Chunk;
using Main.Scripts.LevelGeneration.Data;
using UnityEngine;

namespace Main.Scripts.LevelGeneration
{
public class MapData
{
    public MapGraph MapGraph { get; }
    public IChunk?[][] ChunksMap { get; }
    public List<Vector3> PlayerSpawnPositions { get; }
    public PlaceTargetData FinishPlaceData { get; }

    public MapData(
        MapGraph mapGraph,
        IChunk?[][] chunksMap,
        List<Vector3> playerSpawnPositions,
        PlaceTargetData finishPlaceData
    )
    {
        MapGraph = mapGraph;
        ChunksMap = chunksMap;
        PlayerSpawnPositions = playerSpawnPositions;
        FinishPlaceData = finishPlaceData;
    }
}
}