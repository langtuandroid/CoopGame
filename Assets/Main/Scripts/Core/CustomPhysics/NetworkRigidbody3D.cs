using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Core.Simulation;
using Main.Scripts.Levels;
using Main.Scripts.Player;
using Main.Scripts.Utils;
using Pathfinding;
using UnityEngine;

namespace Main.Scripts.Core.CustomPhysics
{
    [OrderAfter(typeof(ReceiveTicksManager))]
    public class NetworkRigidbody3D : NetworkTransform, IAfterSpawned, GameLoopListener
    {
        private const float DECREASE_PREDICTED_DELTA_ON_TICK = 0.02f;
        private const float MIN_VELOCITY_FOR_DISABLE_PHYSICS = 0.3f;

        [SerializeField]
        private bool isSimulatePhysicsAlwaysForOwner;

        private new Rigidbody rigidbody = default!;
        private PlayersHolder playersHolder = default!;
        private GameLoopManager gameLoopManager = default!;
        private NetworkRigidbody3DInterpolator interpolator = default!;
        private RichAI? richAI;

        [Networked]
        private Vector3 currentNetworkedPosition { get; set; }
        [Networked]
        private int networkedSimulationIndex { get; set; }

        private Tick lastStateAuthorityReceivedTick;
        private Vector3 lastNetworkedPosition;

        private Vector3 lastPredictedPosition;
        private Vector3 predictedPositionDelta;

        private Vector3 interpolationTargetPosition;

        private Vector3 positionBeforeSimulation;

        private int localSimulationIndex;
        private Tick startPhysicsSimulationTick;
        private int simulationTicksCount;

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.SyncTransformBeforeAllPhase,
            GameLoopPhase.SyncTransformAfterAllPhase
        };

        protected override Vector3 DefaultTeleportInterpolationVelocity => rigidbody.velocity;

        protected override void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.interpolation = RigidbodyInterpolation.None;
            richAI = GetComponent<RichAI>();

            interpolator = new NetworkRigidbody3DInterpolator(InterpolatedErrorCorrectionSettings);

            base.Awake();
        }

        public void AfterSpawned()
        {
            rigidbody.isKinematic = !IsSimulatePhysicsAlways();
            
            lastStateAuthorityReceivedTick = default;
            lastNetworkedPosition = transform.position;

            lastPredictedPosition = transform.position;
            predictedPositionDelta = default;

            interpolationTargetPosition = transform.position;

            playersHolder = LevelContext.Instance.ThrowWhenNull().PlayersHolder;
            gameLoopManager = LevelContext.Instance.ThrowWhenNull().GameLoopManager;
            gameLoopManager.AddListener(this);

            InterpolationDataSource = HasStateAuthority
                ? InterpolationDataSources.Predicted
                : InterpolationDataSources.NoInterpolation;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            gameLoopManager.RemoveListener(this);
        }

        public override void Render()
        {
            base.Render();
            if (HasStateAuthority) return;

            if (InterpolationTarget != null)
            {
                var targetInterpolationDelta = transform.position - interpolationTargetPosition;
                interpolator.ApplyNewInterpolationDelta(ref targetInterpolationDelta);

                interpolationTargetPosition = transform.position - targetInterpolationDelta;
                InterpolationTarget.position = interpolationTargetPosition;
            }
        }

        protected override void CopyFromBufferToEngine()
        {
            base.CopyFromBufferToEngine();
            if (HasStateAuthority || playersHolder == null || !playersHolder.Contains(Object.StateAuthority)) return;

            var receiveTickManager = playersHolder.Get(Object.StateAuthority).GetInterface<ReceiveTicksManager>();
            if (receiveTickManager == null) return;

            var curStateAuthorityReceivedTick = receiveTickManager.GetLastReceiveTick(Object.StateAuthority);

            if (lastStateAuthorityReceivedTick < curStateAuthorityReceivedTick)
            {
                if (networkedSimulationIndex >= localSimulationIndex)
                {
                    localSimulationIndex = networkedSimulationIndex;
                    rigidbody.isKinematic = true;
                    predictedPositionDelta = Vector3.zero;
                }
                
                var networkedPositionDelta = currentNetworkedPosition - lastNetworkedPosition;

                if (predictedPositionDelta.magnitude > DECREASE_PREDICTED_DELTA_ON_TICK)
                {
                    var dotNetworkedDelta = Vector3.Dot(networkedPositionDelta, predictedPositionDelta.normalized);
                    DecreasePredictedDelta(dotNetworkedDelta);
                }

                lastNetworkedPosition = currentNetworkedPosition;
                lastStateAuthorityReceivedTick = curStateAuthorityReceivedTick;
            }
            else
            {
                transform.position = lastPredictedPosition;
            }
        }

        public void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.SyncTransformBeforeAllPhase:
                    OnSyncTransformBeforeAllPhase();
                    break;
                case GameLoopPhase.SyncTransformAfterAllPhase:
                    OnSyncTransformAfterAllPhase();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        public IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
        }

        public void AddForce(Vector3 force)
        {
            if (HasStateAuthority)
            {
                networkedSimulationIndex = localSimulationIndex;
                startPhysicsSimulationTick = Runner.Tick;
                var velocity = force.magnitude / rigidbody.mass;
                simulationTicksCount = (int)Math.Ceiling(
                    (Math.Log(MIN_VELOCITY_FOR_DISABLE_PHYSICS, Math.E) - Math.Log(velocity, Math.E)) /
                    Math.Log(1 - Runner.DeltaTime * rigidbody.drag, Math.E)
                );
            }
            
            localSimulationIndex++;
            
            rigidbody.isKinematic = false;
            rigidbody.AddForce(force, ForceMode.Impulse);
        }

        private bool IsSimulatePhysicsAlways()
        {
            return HasStateAuthority && isSimulatePhysicsAlwaysForOwner;
        }

        private void OnSyncTransformBeforeAllPhase()
        {
            if (HasStateAuthority) return;

            transform.position = currentNetworkedPosition + predictedPositionDelta;
            positionBeforeSimulation = transform.position;
        }

        private void OnSyncTransformAfterAllPhase()
        {
            var isLocalSimulation = IsForceRunning();
            
            if (HasStateAuthority)
            {
                currentNetworkedPosition = transform.position;

                if (isLocalSimulation && Runner.Tick - startPhysicsSimulationTick >= simulationTicksCount)
                {
                    networkedSimulationIndex = localSimulationIndex;
                    rigidbody.isKinematic = !IsSimulatePhysicsAlways();
                    
                    if (richAI != null)
                    {
                        richAI.Teleport(transform.position);
                    }
                }
                return;
            }

            if (isLocalSimulation)
            {
                predictedPositionDelta += transform.position - positionBeforeSimulation;
            }

            lastPredictedPosition = transform.position;
        }

        public bool IsForceRunning()
        {
            return localSimulationIndex > networkedSimulationIndex;
        }

        private void DecreasePredictedDelta(float decreaseValue)
        {
            if (decreaseValue >= predictedPositionDelta.magnitude)
            {
                predictedPositionDelta = default;
            }
            else
            {
                predictedPositionDelta -= Math.Max(0, decreaseValue) * predictedPositionDelta.normalized;
            }
        }
    }
}