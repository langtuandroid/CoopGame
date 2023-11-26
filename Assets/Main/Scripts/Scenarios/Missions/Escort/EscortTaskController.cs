using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Units;
using UnityEngine;

namespace Main.Scripts.Scenarios.Missions.Escort
{
public class EscortTaskController : GameLoopEntityNetworked, CartController.Listener
{
    [SerializeField]
    private CartController cartPrefab = null!;

    private List<Vector3> pathPoints = null!;
    private Listener? listener;

    private CartController? cartController;

    private GameLoopPhase[] gameLoopPhases =
    {
        GameLoopPhase.ObjectsSpawnPhase
    };

    public void Init(
        List<Vector3> pathPoints,
        Listener listener
    )
    {
        this.pathPoints = pathPoints;
        this.listener = listener;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        pathPoints = null!;
        cartController = null;
        listener = null!;
    }

    public override void OnGameLoopPhase(GameLoopPhase phase)
    {
        if (!HasStateAuthority) return;

        switch (phase)
        {
            case GameLoopPhase.ObjectsSpawnPhase:
                OnSpawnPhase();
                break;
        }
    }

    public override IEnumerable<GameLoopPhase> GetSubscribePhases()
    {
        return gameLoopPhases;
    }

    private void OnSpawnPhase()
    {
        if (cartController != null) return;

        cartController = Runner.Spawn(
            prefab: cartPrefab,
            position: pathPoints[0],
            rotation: Quaternion.LookRotation(pathPoints[1] - pathPoints[0], Vector3.up)
        );
        cartController.Init(
            pathPoints: pathPoints,
            listener: this
        );
    }

    public void OnDead()
    {
        listener?.OnFailed();
    }

    public void OnFinishSuccess()
    {
        listener?.OnSuccess();
    }

    public interface Listener
    {
        public void OnSuccess();
        public void OnFailed();
    }
}
}