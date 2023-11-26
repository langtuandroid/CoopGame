using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using UnityEngine;

namespace Main.Scripts.Units
{
public class CartController : GameLoopEntityNetworked
{
    [SerializeField]
    [Min(0f)]
    private float speed;
    [SerializeField]
    private Vector2 checkFrontSpaceBounds;
    [SerializeField]
    private float checkFrontSpaceOffset;
    [SerializeField]
    private LayerMask checkFrontSpaceLayerMask;

    private List<Vector3> pathPoints = null!;
    private Listener? listener = null!;

    private Transform? cartTransform;
    private int currentPathPointIndex;
    private float currentRoadProgress;

    private Collider[] colliders = new Collider[1];

    private GameLoopPhase[] gameLoopPhases =
    {
        GameLoopPhase.PhysicsUpdatePhase
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
        listener = null;
    }

    public override void OnGameLoopPhase(GameLoopPhase phase)
    {
        if (!HasStateAuthority) return;

        switch (phase)
        {
            case GameLoopPhase.PhysicsUpdatePhase:
                OnPhysicsUpdatePhase();
                break;
        }
    }

    public override IEnumerable<GameLoopPhase> GetSubscribePhases()
    {
        return gameLoopPhases;
    }

    private void OnPhysicsUpdatePhase()
    {
        if (currentPathPointIndex == pathPoints.Count - 1) return;

        var checkBounds = checkFrontSpaceBounds;

        var hitsCount = Physics.OverlapBoxNonAlloc(
            center: transform.position + transform.forward * checkFrontSpaceOffset,
            halfExtents: new Vector3(checkBounds.x, 1, checkBounds.y) / 2f,
            results: colliders,
            orientation: Quaternion.identity,
            mask: checkFrontSpaceLayerMask
        );

        if (hitsCount > 0) return;

        var fromPoint = pathPoints[currentPathPointIndex];
        var toPoint = pathPoints[currentPathPointIndex + 1];

        var roadDistance = Vector3.Distance(fromPoint, toPoint);
        currentRoadProgress += speed * Runner.DeltaTime / roadDistance;

        transform.position = Vector3.Lerp(fromPoint, toPoint, Math.Min(currentRoadProgress, 1f));
        transform.rotation = Quaternion.LookRotation(toPoint - fromPoint, Vector3.up);


        if (currentRoadProgress >= 1f)
        {
            currentPathPointIndex++;
            currentRoadProgress = 0f;
        }

        if (currentPathPointIndex == pathPoints.Count - 1)
        {
            listener?.OnFinishSuccess();
        }
    }

    public interface Listener
    {
        public void OnDead();
        public void OnFinishSuccess();
    }
}
}