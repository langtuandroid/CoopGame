using System;
using Fusion;
using Main.Scripts.Enemies;
using Main.Scripts.Levels;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Player.InputSystem.Target
{
    public class FindTargetManager : MonoBehaviour
    {
        public static FindTargetManager? Instance { get; private set; }

        [SerializeField]
        private float maxTargetDistance = 20;
        [SerializeField]
        private float snapDistance = 3;
        [SerializeField]
        private LayerMask mouseRayMask;
        [SerializeField]
        private LayerMask opponentsLayerMask;
        [SerializeField]
        private LayerMask alliesLayerMask;

        private PlayersHolder playersHolder = default!;
        private EnemiesManager enemiesManager = default!;


        private UnitTargetType targetType;
        private PlayerRef ownerRef;

        public FindTargetState State { get; private set; } = FindTargetState.NOT_ACTIVE;
        public NetworkObject? FocusedTarget { get; private set; }

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Start()
        {
            var levelContext = LevelContext.Instance.ThrowWhenNull();
            playersHolder = levelContext.PlayersHolder;
            enemiesManager = levelContext.EnemiesManager;
        }

        private void Update()
        {
            switch (State)
            {
                case FindTargetState.NOT_ACTIVE:
                    break;
                case FindTargetState.ACTIVE:
                    UpdateFocusedTarget();
                    break;
                case FindTargetState.SELECTED:
                    UpdateFocusedTarget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool TryActivate(PlayerRef ownerRef, UnitTargetType targetType, out NetworkObject? foundTarget)
        {
            foundTarget = null;
            switch (State)
            {
                case FindTargetState.NOT_ACTIVE:
                    Activate(ownerRef, targetType);
                    if (FocusedTarget != null)
                    {
                        foundTarget = FocusedTarget;
                    }

                    return true;
                case FindTargetState.ACTIVE:
                    break;
                case FindTargetState.SELECTED:
                    Activate(ownerRef, targetType);
                    if (FocusedTarget != null)
                    {
                        foundTarget = FocusedTarget;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        public void StopActive(bool resetTarget)
        {
            State = FindTargetState.NOT_ACTIVE;
            if (resetTarget)
            {
                if (FocusedTarget != null)
                {
                    FocusedTarget.GetComponent<SelectionTargetMarker>()?.SetTargetFocusState(TargetFocusState.NONE);
                }

                FocusedTarget = null;
            }
        }

        private void Activate(PlayerRef ownerRef, UnitTargetType targetType)
        {
            State = FindTargetState.ACTIVE;
            this.targetType = targetType;
            this.ownerRef = ownerRef;

            if (FocusedTarget != null)
            {
                FocusedTarget.GetComponent<SelectionTargetMarker>()?.SetTargetFocusState(TargetFocusState.NONE);
            }

            UpdateFocusedTarget();

            if (FocusedTarget != null)
            {
                FocusedTarget.GetComponent<SelectionTargetMarker>()?.SetTargetFocusState(TargetFocusState.FOCUSED);
            }
        }

        private NetworkObject? TryFocusNearestToMapPointTarget()
        {
            if (ownerRef == default) return null;

            NetworkObject? nearestTarget = null;
            var targetDistance = 0f;

            var mapPoint = MousePositionHelper.GetMapPoint(mouseRayMask);

            if (targetType is UnitTargetType.Allies or UnitTargetType.All)
            {
                var playerRefs = playersHolder.GetKeys();

                foreach (var playerRef in playerRefs)
                {
                    if (playerRef == ownerRef) continue;
                    var player = playersHolder.Get(playerRef);

                    var distance = Vector3.Distance(mapPoint, player.transform.position);

                    if (distance < snapDistance && (nearestTarget == null || targetDistance > distance))
                    {
                        nearestTarget = player.Object;
                        targetDistance = distance;
                    }
                }
            }

            if (targetType is UnitTargetType.Opponents or UnitTargetType.All)
            {
                var enemies = enemiesManager.GetEnemies();

                foreach (var enemy in enemies)
                {
                    var distance = Vector3.Distance(mapPoint, enemy.transform.position);

                    if (distance < snapDistance && (nearestTarget == null || targetDistance > distance))
                    {
                        nearestTarget = enemy.Object;
                        targetDistance = distance;
                    }
                }
            }

            return targetDistance <= maxTargetDistance ? nearestTarget : null;
        }

        private void UpdateFocusedTarget()
        {
            var mapPoint = MousePositionHelper.GetMapPoint(mouseRayMask);

            if (State != FindTargetState.SELECTED
                || FocusedTarget == null
                || !IsTargetType(FocusedTarget.gameObject.layer)
                || Vector3.Distance(FocusedTarget.transform.position, mapPoint) > maxTargetDistance)
            {
                var newTarget = TryFocusNearestToMapPointTarget();
                if (FocusedTarget != newTarget)
                {
                    if (FocusedTarget != null)
                    {
                        FocusedTarget.GetComponent<SelectionTargetMarker>()?.SetTargetFocusState(TargetFocusState.NONE);
                    }

                    if (newTarget != null)
                    {
                        newTarget.GetComponent<SelectionTargetMarker>()?.SetTargetFocusState(TargetFocusState.FOCUSED);
                    }
                }

                FocusedTarget = newTarget;
            }
        }

        private bool IsTargetType(LayerMask layerMask)
        {
            return targetType switch
            {
                UnitTargetType.Opponents => layerMask == opponentsLayerMask,
                UnitTargetType.Allies => layerMask == alliesLayerMask,
                UnitTargetType.All => layerMask == opponentsLayerMask || layerMask == alliesLayerMask,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}