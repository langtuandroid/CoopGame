using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Main.Scripts.Scenarios.Missions.Common
{
public class PlayersSpawnScenario : Scenario
{
    private NetworkRunner runner;
    private PlayersSpawnTaskController playersSpawnTaskPrefab;
    private List<Vector3> spawnPoints;

    private PlayersSpawnTaskController playersSpawnTaskController;
    private ScenarioStatus status;

    public PlayersSpawnScenario(
        NetworkRunner runner,
        PlayersSpawnTaskController playersSpawnTaskPrefab,
        List<Vector3> spawnPoints
    )
    {
        this.runner = runner;
        this.playersSpawnTaskPrefab = playersSpawnTaskPrefab;
        this.spawnPoints = spawnPoints;
    }

    public void Start()
    {
        playersSpawnTaskController = runner.Spawn(
            prefab: playersSpawnTaskPrefab
        );

        playersSpawnTaskController.OnAllPlayersSpawned = OnAllPlayersSpawned;
        playersSpawnTaskController.SpawnPlayers(spawnPoints);

        status = ScenarioStatus.InProgress;
    }

    public void Stop()
    {
        if (status == ScenarioStatus.None) return;

        runner.Despawn(playersSpawnTaskController.Object);
    }

    public void Update() { }

    public ScenarioStatus GetStatus()
    {
        return status;
    }

    private void OnAllPlayersSpawned()
    {
        playersSpawnTaskController.OnAllPlayersSpawned = null;

        status = ScenarioStatus.Success;
    }
}
}