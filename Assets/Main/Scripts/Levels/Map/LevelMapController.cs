using System;
using System.Collections.Generic;
using Main.Scripts.LevelGeneration;
using Main.Scripts.LevelGeneration.Chunk;
using Main.Scripts.LevelGeneration.Configs;
using Main.Scripts.LevelGeneration.NavMesh;
using Main.Scripts.LevelGeneration.Places.Crossroads;
using Main.Scripts.LevelGeneration.Places.Outside;
using Main.Scripts.LevelGeneration.Places.Road;
using Pathfinding;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Main.Scripts.Levels.Map
{
public class LevelMapController
{
    private AstarPath pathfinder;
    private IChunk[][] chunksMap = null!;
    private NavMeshChunkBuilder navMeshChunkBuilder = new();
    private LevelGenerator levelGenerator = new();
    private LevelGenerationConfig levelGenerationConfig;
    private LevelStyleConfig levelStyleConfig;

    private Dictionary<Vector2Int, GameObject> spawnedObjectsMap = new();

    private int spawnedFromXIndex = 0;
    private int spawnedToXIndex = 0;
    private int spawnedFromYIndex = 0;
    private int spawnedToYIndex = 0;

    public LevelMapController(
        LevelGenerationConfig levelGenerationConfig,
        LevelStyleConfig levelStyleConfig,
        AstarPath pathfinder
    )
    {
        this.levelGenerationConfig = levelGenerationConfig;
        this.levelStyleConfig = levelStyleConfig;
        this.pathfinder = pathfinder;
    }

    public void GenerateMap(int seed)
    {
        foreach (var (_, spawnedObject) in spawnedObjectsMap)
        {
            Object.Destroy(spawnedObject);
        }

        spawnedObjectsMap.Clear();

        spawnedFromXIndex = 0;
        spawnedToXIndex = 0;
        spawnedFromYIndex = 0;
        spawnedToYIndex = 0;

        chunksMap = levelGenerator.Generate(
            seed,
            levelGenerationConfig,
            levelStyleConfig.DecorationsPack
        );

        GenerateNavMesh(chunksMap);
    }

    private void GenerateNavMesh(IChunk[][] map)
    {
        var navMesh = navMeshChunkBuilder.GenerateNavMesh(map, levelGenerationConfig.ChunkSize);

        for (var i = 0; i < pathfinder.graphs.Length; i++)
        {
            if (pathfinder.graphs[i] is NavMeshGraph navMeshGraph)
            {
                navMeshGraph.sourceMesh = navMesh;
                navMeshGraph.Scan();
                break;
            }
        }
    }

    public void UpdateChunksVisibilityBounds(
        float minX,
        float maxX,
        float minY,
        float maxY
    )
    {
        var chunkSize = levelGenerationConfig.ChunkSize;

        var fromXIndex = (int)Math.Max(0, Math.Floor(minX / chunkSize));
        var toXIndex = (int)Math.Min(chunksMap.Length, Math.Ceiling(maxX / chunkSize) + 1);
        var fromYIndex = (int)Math.Max(0, Math.Floor(minY / chunkSize));
        var toYIndex = (int)Math.Min(chunksMap[0].Length, Math.Ceiling(maxY / chunkSize) + 1);


        for (var x = spawnedFromXIndex; x < spawnedToXIndex; x++)
        {
            for (var y = spawnedFromYIndex; y < spawnedToYIndex; y++)
            {
                if ((x < fromXIndex
                     || x >= toXIndex
                     || y < fromYIndex
                     || y >= toYIndex)
                    && spawnedObjectsMap.Remove(new Vector2Int(x, y), out var spawnedObject))
                {
                    Object.Destroy(spawnedObject);
                }
            }
        }

        SpawnPrefabs(
            fromXIndex: fromXIndex,
            toXIndex: toXIndex,
            fromYIndex: fromYIndex,
            toYIndex: toYIndex
        );

        spawnedFromXIndex = fromXIndex;
        spawnedToXIndex = toXIndex;
        spawnedFromYIndex = fromYIndex;
        spawnedToYIndex = toYIndex;
    }

    private void SpawnPrefabs(
        int fromXIndex,
        int toXIndex,
        int fromYIndex,
        int toYIndex
    )
    {
        var chunkSize = levelGenerationConfig.ChunkSize;
        for (var x = fromXIndex; x < toXIndex; x++)
        {
            for (var y = fromYIndex; y < toYIndex; y++)
            {
                if (spawnedObjectsMap.ContainsKey(new Vector2Int(x, y)))
                {
                    continue;
                }

                switch (chunksMap[x][y])
                {
                    case RoadChunk roadChunk:
                        var roadController = Object.Instantiate(
                            original: levelStyleConfig.RoadChunkPrefab,
                            position: new Vector3(x * chunkSize, 0, y * chunkSize),
                            rotation: Quaternion.identity
                        );

                        roadController.Init(
                            roadChunk,
                            chunkSize
                        );

                        spawnedObjectsMap.Add(new Vector2Int(x, y), roadController.gameObject);
                        break;
                    case CrossroadsChunk crossroadsChunk:
                        var crossroadsController = Object.Instantiate(
                            original: levelStyleConfig.CrossroadsChunkPrefab,
                            position: new Vector3(x * chunkSize, 0, y * chunkSize),
                            rotation: Quaternion.identity
                        );

                        crossroadsController.Init(
                            crossroadsChunk,
                            chunkSize
                        );

                        spawnedObjectsMap.Add(new Vector2Int(x, y), crossroadsController.gameObject);
                        break;
                    case OutsideChunk outsideChunk:
                        var chunkConnectionTypes =
                            ChunkHelper.GetChunkConnectionTypes(chunksMap, x, y,
                                chunk => ChunkHelper.IsLowerHeightLevel(chunk, outsideChunk.HeightLevel));

                        var outsideChunkController = Object.Instantiate(
                            original: levelStyleConfig.HillOutsideChunkPrefab,
                            position: new Vector3(x * chunkSize, 0, y * chunkSize),
                            rotation: Quaternion.identity
                        );
                        outsideChunkController.Init(outsideChunk, chunkConnectionTypes);
                        spawnedObjectsMap.Add(new Vector2Int(x, y), outsideChunkController.gameObject);
                        break;
                }
            }
        }
    }
}
}