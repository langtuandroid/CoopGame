using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Main.Scripts.Scenarios.Missions.Escort
{
public class EscortScenario : Scenario, EscortTaskController.Listener
{
    private NetworkRunner runner;
    private EscortTaskController escortTaskPrefab;
    private List<Vector3> pathPoints;

    private ScenarioStatus status;
    private EscortTaskController escortTaskController;

    public EscortScenario(
        NetworkRunner runner,
        EscortTaskController escortTaskPrefab,
        List<Vector3> pathPoints
    )
    {
        this.runner = runner;
        this.escortTaskPrefab = escortTaskPrefab;
        this.pathPoints = pathPoints;
    }

    public void Start()
    {
        escortTaskController = runner.Spawn(
            prefab: escortTaskPrefab
        );

        escortTaskController.Init(
            pathPoints: pathPoints,
            listener: this
        );

        status = ScenarioStatus.InProgress;
    }

    public void Stop()
    {
        if (status == ScenarioStatus.None) return;

        runner.Despawn(escortTaskController.Object);

        runner = null!;
        escortTaskController = null!;
        pathPoints = null!;
        status = ScenarioStatus.None;
    }

    public void Update() { }

    public ScenarioStatus GetStatus()
    {
        return status;
    }

    public void OnSuccess()
    {
        status = ScenarioStatus.Success;
    }

    public void OnFailed()
    {
        status = ScenarioStatus.Failed;
    }
}
}