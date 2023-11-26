using System.Collections.Generic;
using Fusion;
using Main.Scripts.LevelGeneration;
using Main.Scripts.Player;
using Main.Scripts.Scenarios.Missions.Common;
using UnityEngine;

namespace Main.Scripts.Scenarios.Missions.Escort
{
public class EscortMissionScenarioGenerator : MissionScenarioGenerator
{
    private EscortScenarioGeneratorConfig config;
    private NetworkRunner runner;
    private PlayersHolder playersHolder;
    private MapData mapData;

    public EscortMissionScenarioGenerator(
        EscortScenarioGeneratorConfig config,
        NetworkRunner runner,
        PlayersHolder playersHolder,
        MapData mapData
    )
    {
        this.config = config;
        this.runner = runner;
        this.playersHolder = playersHolder;
        this.mapData = mapData;
    }

    public Scenario GenerateScenario()
    {
        var spawnScenario = new PlayersSpawnScenario(
            runner: runner,
            playersSpawnTaskPrefab: config.PlayersSpawnTaskPrefab,
            mapData.PlayerSpawnPositions
        );

        var pathPoints = new List<Vector3>();
        var chunkSize = mapData.ChunkSize;

        foreach (var roadData in mapData.MapGraph.Roads)
        {
            if (pathPoints.Count == 0)
            {
                var firstPointPosition = mapData.MapGraph.Places[roadData.FromIndex].Position;
                pathPoints.Add(new Vector3(firstPointPosition.x, 0, firstPointPosition.y) * chunkSize);
            }

            var position = mapData.MapGraph.Places[roadData.ToIndex].Position;
            pathPoints.Add(new Vector3(position.x, 0, position.y) * chunkSize);
        }

        var escortScenario = new EscortScenario(
            runner: runner,
            escortTaskPrefab: config.EscortTaskPrefab,
            pathPoints: pathPoints
        );

        var playersAliveScenario = new PlayersAliveScenario(
            playersHolder: playersHolder
        );

        var anyCompletedList = new List<Scenario>();
        anyCompletedList.Add(playersAliveScenario);
        anyCompletedList.Add(escortScenario);

        var anyCompletedScenarioGroup = new ScenariosAnyCompletedGroup(anyCompletedList);

        var scenariosChain = new List<Scenario>();
        scenariosChain.Add(spawnScenario);
        scenariosChain.Add(anyCompletedScenarioGroup);

        return new ScenariosChainGroup(scenariosChain);
    }
}
}