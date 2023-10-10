using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Levels;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component;
using Main.Scripts.Skills.Common.Component.Config;
using Main.Scripts.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Main.Scripts.Skills.Common.Controller
{
    public class SkillController
    {
        private NetworkObject objectContext = null!;
        private SkillControllerConfig skillControllerConfig;
        private PlayerRef ownerId;

        private Listener? listener;
        private GameObject? marker;
        private SkillComponentsPoolHelper skillComponentsPoolHelper = null!;

        private Vector3 initialMapPoint;
        private Vector3 dynamicMapPoint;
        private NetworkObject? selectedUnit;
        private TickTimer skillRunningTimer;
        private TickTimer castTimer;
        private bool isActivating;
        private int chargeLevel;

        private TickTimer cooldownTimer;
        
        private HashSet<SkillComponent> skillComponents = new();
        
        private Transform selfUnitTransform;
        private LayerMask alliesLayerMask;
        private LayerMask opponentsLayerMask;

        private List<SkillConfig> spawnActions = new();

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.SkillUpdatePhase,
            GameLoopPhase.SkillSpawnPhase,
            GameLoopPhase.VisualStateUpdatePhase
        };

        public bool IsSkillRunning => skillRunningTimer.IsRunning;
        public SkillActivationType ActivationType => skillControllerConfig.ActivationType;
        public UnitTargetType SelectionTargetType => skillControllerConfig.SelectionTargetType;

        public SkillController(
            SkillControllerConfig skillControllerConfig,
            Transform selfUnitTransform,
            LayerMask alliesLayerMask,
            LayerMask opponentsLayerMask
        )
        {
            this.skillControllerConfig = skillControllerConfig;
            this.selfUnitTransform = selfUnitTransform;
            this.alliesLayerMask = alliesLayerMask;
            this.opponentsLayerMask = opponentsLayerMask;
        }

        public void Spawned(NetworkObject objectContext, bool isPlayerOwner)
        {
            this.objectContext = objectContext;
            ownerId = isPlayerOwner ? objectContext.StateAuthority : default;
            skillComponentsPoolHelper = LevelContext.Instance.ThrowWhenNull().SkillComponentsPoolHelper;
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            ResetOnFinish();
            
            objectContext = null!;
            skillComponentsPoolHelper = null!;
            cooldownTimer = default!;

            spawnActions.Clear();
        }

        public void Render()
        {
            //todo вынести в отдельный класс, который будет создаваться только для локального игрока
            if (objectContext.Runner.LocalPlayer != objectContext.StateAuthority) return;

            var canShowMarker = skillControllerConfig.ActivationType != SkillActivationType.WithUnitTarget ||
                                selectedUnit != null;
            if (isActivating && canShowMarker)
            {
                var markerPosition = selectedUnit != null && skillControllerConfig.ActivationType == SkillActivationType.WithUnitTarget
                        ? selectedUnit.transform.position
                        : initialMapPoint;
                    
                if (marker == null)
                {
                    if (skillControllerConfig.AreaMarker != null)
                    {
                        marker = Object.Instantiate(
                            original: skillControllerConfig.AreaMarker,
                            position: new Vector3(markerPosition.x, 0.001f, markerPosition.z),
                            rotation: skillControllerConfig.AreaMarker.transform.rotation
                        );
                    }
                }

                if (marker != null)
                {
                    if (!marker.activeSelf)
                    {
                        marker.SetActive(true);
                    }

                    marker.transform.position = new Vector3(markerPosition.x, 0.001f, markerPosition.z);
                }
            }
            else if (marker != null && marker.activeSelf)
            {
                marker.SetActive(false);
            }
        }

        public bool Activate(int chargeLevel)
        {
            if (isActivating || IsSkillRunning || !cooldownTimer.ExpiredOrNotRunning(objectContext.Runner))
            {
                return false;
            }

            isActivating = true;
            this.chargeLevel = chargeLevel;

            switch (skillControllerConfig.ActivationType)
            {
                case SkillActivationType.Instantly:
                    Execute();
                    break;
                case SkillActivationType.WithMapPointTarget:
                    listener?.OnSkillWaitingForPointTarget(this);
                    break;
                case SkillActivationType.WithUnitTarget:
                    listener?.OnSkillWaitingForUnitTarget(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        public void UpdateMapPoint(Vector3 mapPoint)
        {
            dynamicMapPoint = mapPoint;
            if (!IsSkillRunning)
            {
                initialMapPoint = mapPoint;
            }

            foreach (var skillComponent in skillComponents)
            {
                skillComponent.UpdateMapPoint(mapPoint);
            }
        }

        public void ApplyUnitTarget(NetworkObject unitTarget)
        {
            if (!isActivating) return;

            selectedUnit = unitTarget;
        }

        public void Execute()
        {
            if (!isActivating)
            {
                throw new Exception("Skill must be activated before calling execute");
            }

            if (IsSkillRunning) return;

            isActivating = false;

            skillRunningTimer = TickTimer.CreateFromTicks(objectContext.Runner, skillControllerConfig.CastDurationTicks + skillControllerConfig.ExecutionDurationTicks);
            castTimer = TickTimer.CreateFromTicks(objectContext.Runner, skillControllerConfig.CastDurationTicks);

            listener?.OnSkillStartCasting(this);

            AddSpawnActions(skillControllerConfig.RunOnCastSkillConfigs);

            CheckCastFinished();
        }

        public void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.SkillSpawnPhase:
                    OnSpawnPhase();
                    break;
                case GameLoopPhase.SkillUpdatePhase:
                    OnSkillUpdatePhase();
                    break;
                case GameLoopPhase.VisualStateUpdatePhase:
                    OnVisualStateUpdatePhase();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        public IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            //todo remove
            return gameLoopPhases;
        }

        private void OnSkillUpdatePhase()
        {
            CheckCastFinished();

            if (skillRunningTimer.Expired(objectContext.Runner))
            {
                ResetOnFinish();

                listener?.OnSkillFinished(this);
            }
        }

        private void OnSpawnPhase()
        {
            SpawnSkills(spawnActions);
            spawnActions.Clear();
        }

        private void OnVisualStateUpdatePhase()
        {
            if (cooldownTimer.IsRunning)
            {
                if (cooldownTimer.Expired(objectContext.Runner))
                {
                    cooldownTimer = default;
                }

                listener?.OnSkillCooldownChanged(this);
            }
        }

        private void CheckCastFinished()
        {
            if (castTimer.Expired(objectContext.Runner))
            {
                castTimer = default;
                cooldownTimer = TickTimer.CreateFromTicks(objectContext.Runner, skillControllerConfig.CooldownTicks);
                AddSpawnActions(skillControllerConfig.RunOnExecutionSkillConfigs);
                
                listener?.OnSkillFinishedCasting(this);
            }
        }

        public int GetCooldownLeftTicks()
        {
            return cooldownTimer.RemainingTicks(objectContext.Runner) ?? 0;
        }

        //todo
        public void OnClickTrigger()
        {
            foreach (var skillComponent in skillComponents)
            {
                skillComponent.OnClickTrigger();
            }
        }

        public bool TryInterrupt()
        {
            //todo доделать
            foreach (var skillComponent in skillComponents)
            {
                skillComponent.TryInterrupt();
            }

            return false;
        }

        public void CancelActivation()
        {
            if (!isActivating) return;

            ResetOnFinish();

            listener?.OnSkillCanceled(this);
        }

        public bool IsDisabledMove()
        {
            var isDisableOnCast = skillControllerConfig.DisableMoveOnCast && !castTimer.ExpiredOrNotRunning(objectContext.Runner);
            var isDisableOnExecution = skillControllerConfig.DisableMoveOnExecution && IsSkillRunning;
            return isDisableOnCast || isDisableOnExecution;
        }

        private void ResetOnFinish()
        {
            castTimer = default;
            skillRunningTimer = default;
            isActivating = false;

            selectedUnit = null;
            initialMapPoint = default;
            dynamicMapPoint = default;

            foreach (var skillComponent in skillComponents)
            {
                skillComponent.OnLostControl();
                skillComponent.OnReadyToRelease -= OnReadyToReleaseSkillComponent;
            }
            skillComponents.Clear();
        }

        public void SetListener(Listener? listener)
        {
            this.listener = listener;
        }

        private void AddSpawnActions(IEnumerable<SkillConfig> skillConfigs)
        {
            spawnActions.AddRange(skillConfigs);
        }

        private void SpawnSkills(List<SkillConfig> skillConfigs)
        {
            foreach (var skillConfig in skillConfigs)
            {
                Vector3 position;
                Quaternion rotation;
                switch (skillConfig.SpawnPointType)
                {
                    case SkillSpawnPointType.SelfUnitTarget:
                        position = selfUnitTransform.transform.position;
                        break;
                    case SkillSpawnPointType.SelectedUnitTarget:
                        selectedUnit.ThrowWhenNull();
                        position = selectedUnit.transform.position;
                        break;
                    case SkillSpawnPointType.InitialMapPointTarget:
                        position = initialMapPoint;
                        break;
                    case SkillSpawnPointType.DynamicMapPointTarget:
                        position = dynamicMapPoint;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (skillConfig.SpawnDirectionType)
                {
                    case SkillSpawnDirectionType.ToSelfUnit:
                        rotation = Quaternion.LookRotation(selfUnitTransform.transform.position - position);
                        break;
                    case SkillSpawnDirectionType.ToSelectedUnit:
                        selectedUnit.ThrowWhenNull();
                        rotation = Quaternion.LookRotation(selectedUnit.transform.position - position);
                        break;
                    case SkillSpawnDirectionType.ToInitialMapPoint:
                        rotation = Quaternion.LookRotation(initialMapPoint - position);
                        break;
                    case SkillSpawnDirectionType.ToDynamicMapPoint:
                        rotation = Quaternion.LookRotation(dynamicMapPoint - position);
                        break;
                    case SkillSpawnDirectionType.SelfUnitLookDirection:
                        rotation = selfUnitTransform.transform.rotation;
                        break;
                    case SkillSpawnDirectionType.SelfUnitMoveDirection:
                        rotation = Quaternion.LookRotation(
                            selfUnitTransform.GetInterface<Movable>()?.GetMovingDirection() ?? Vector3.zero
                        );
                        break;
                    case SkillSpawnDirectionType.SelectedUnitLookDirection:
                        selectedUnit.ThrowWhenNull();
                        rotation = selectedUnit.transform.rotation;
                        break;
                    case SkillSpawnDirectionType.SelectedUnitMoveDirection:
                        selectedUnit.ThrowWhenNull();
                        rotation = Quaternion.LookRotation(
                            selectedUnit.GetInterface<Movable>()?.GetMovingDirection() ?? Vector3.zero
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var skillComponent = skillComponentsPoolHelper.Get();
                skillComponent.Init(
                    skillConfig: skillConfig,
                    runner: objectContext.Runner,
                    position: position,
                    rotation: rotation,
                    ownerId: ownerId,
                    chargeLevel: chargeLevel,
                    initialMapPoint: initialMapPoint,
                    dynamicMapPoint: dynamicMapPoint,
                    selfUnitId: objectContext.Id,
                    selectedUnitId: selectedUnit != null ? selectedUnit.Id : default,
                    alliesLayerMask: alliesLayerMask,
                    opponentsLayerMask: opponentsLayerMask,
                    onSpawnNewSkillComponent: OnSpawnNewSkillComponent
                );

                skillComponents.Add(skillComponent);
            }
        }

        private void OnSpawnNewSkillComponent(SkillComponent spawnedSkillComponent)
        {
            skillComponents.Add(spawnedSkillComponent);
        }

        private void OnReadyToReleaseSkillComponent(SkillComponent skillComponent)
        {
            skillComponents.Remove(skillComponent);
        }

        public interface Listener
        {
            public void OnSkillCooldownChanged(SkillController skill);
            public void OnSkillWaitingForPointTarget(SkillController skill);
            public void OnSkillWaitingForUnitTarget(SkillController skill);
            public void OnSkillStartCasting(SkillController skill);
            public void OnSkillFinishedCasting(SkillController skill);
            public void OnSkillFinished(SkillController skill);
            public void OnSkillCanceled(SkillController skill);
        }
    }
}