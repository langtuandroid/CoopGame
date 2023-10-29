using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Levels;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component;
using Main.Scripts.Skills.Common.Component.Config;
using Main.Scripts.Skills.Common.Controller.Interruption;
using Main.Scripts.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Main.Scripts.Skills.Common.Controller
{
    public class SkillController
    {
        private NetworkObject objectContext = null!;
        private SkillControllerConfig skillControllerConfig = null!;
        private PlayerRef ownerId;

        private Listener? listener;
        private GameObject? marker;
        private SkillComponentsPoolHelper skillComponentsPoolHelper = null!;

        private Vector3 initialMapPoint;
        private Vector3 dynamicMapPoint;
        private int powerChargeLevel;
        private NetworkId selectedUnitId;
        private NetworkObject? selectedUnit;
        private List<NetworkId> effectTargetsIdList = new();
        private int heatLevel;
        private int stackCount;
        private TickTimer executionTimer;
        private bool continueRunningWhileHolding;
        private TickTimer castTimer;
        private bool isActivating;
        private Tick castStartTick;

        private TickTimer cooldownTimer;
        
        private HashSet<SkillComponent> skillComponents = new();
        
        private Transform selfUnitTransform = null!;
        private LayerMask alliesLayerMask;
        private LayerMask opponentsLayerMask;

        private List<SkillConfig> spawnActions = new();

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.SkillCheckSkillFinished,
            GameLoopPhase.SkillCheckCastFinished,
            GameLoopPhase.SkillSpawnPhase,
            GameLoopPhase.VisualStateUpdatePhase
        };

        private bool IsSkillRunning => castTimer.IsRunning || executionTimer.IsRunning;
        public SkillActivationType ActivationType => skillControllerConfig.ActivationType;
        public UnitTargetType SelectionTargetType => skillControllerConfig.SelectionTargetType;

        public void Init(
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
        
        
        public void Release()
        {
            skillControllerConfig = null!;
            selfUnitTransform = null!;
            alliesLayerMask = default;
            opponentsLayerMask = default;
        }

        public void Despawned()
        {
            ResetOnFinish();
            
            objectContext = null!;
            skillComponentsPoolHelper = null!;
            cooldownTimer = default;

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

        public bool CanActivate()
        {
            return !isActivating && !IsSkillRunning && cooldownTimer.ExpiredOrNotRunning(objectContext.Runner);
        }

        public bool Activate(int heatLevel, int stackCount, List<NetworkId>? effectTargetsIdList)
        {
            if (!CanActivate())
            {
                return false;
            }

            isActivating = true;
            this.heatLevel = heatLevel;
            this.stackCount = stackCount;
            if (effectTargetsIdList != null)
            {
                this.effectTargetsIdList.AddRange(effectTargetsIdList);
            }

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
            selectedUnitId = unitTarget.Id;
        }

        public void ApplyHolding()
        {
            var holdingType = skillControllerConfig.HoldingType;
            if (!IsSkillRunning || holdingType == SkillHoldingType.None) return;

            continueRunningWhileHolding = true;
            foreach (var skillComponent in skillComponents)
            {
                skillComponent.ApplyHolding();
            }
        }

        public void Execute()
        {
            if (!isActivating)
            {
                throw new Exception("Skill must be activated before calling execute");
            }

            if (IsSkillRunning) return;

            isActivating = false;

            castTimer = TickTimer.CreateFromTicks(objectContext.Runner, skillControllerConfig.CastDurationTicks);
            castStartTick = objectContext.Runner.Tick;
            
            if (skillControllerConfig.CooldownStartType == SkillCooldownStartType.OnCast)
            {
                StartCooldown();
            }
            continueRunningWhileHolding = skillControllerConfig.HoldingType != SkillHoldingType.None;

            listener?.OnSkillStartCasting(this);

            AddSpawnActions(skillControllerConfig.RunOnCastSkillConfigs);
        }

        private int GetPowerChargeProgress()
        {
            if (skillControllerConfig.TicksToFullPowerCharge == 0)
            {
                return 100;
            }
            return Math.Min(
                100,
                (int)(100 * (objectContext.Runner.Tick - castStartTick) / (float)skillControllerConfig.TicksToFullPowerCharge)
            );
        }

        private int GetPowerChargeLevel(int powerChargeProgress)
        {
            var level = 0;
            var powerChargeStepValues = skillControllerConfig.PowerChargeStepValues;
            while (level < powerChargeStepValues.Length && powerChargeProgress >= powerChargeStepValues[level])
            {
                level++;
            }

            return level;
        }

        public void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.SkillCheckSkillFinished:
                    CheckSelectedTargetDead();
                    CheckSkillFinished();
                    break;
                case GameLoopPhase.SkillCheckCastFinished:
                    CheckCastFinished();
                    break;
                case GameLoopPhase.SkillSpawnPhase:
                    OnSpawnPhase();
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

        private void OnSpawnPhase()
        {
            SpawnSkills(spawnActions);
            spawnActions.Clear();
        }

        private void CheckSelectedTargetDead()
        {
            if (!IsSkillRunning || skillControllerConfig.ActivationType != SkillActivationType.WithUnitTarget) return;
            
            if (selectedUnitId.IsValid && (selectedUnit == null || selectedUnit.Id != selectedUnitId))
            {
                TryInterrupt(SkillInterruptionType.SelectedTargetDead);
            }
        }

        private void CheckSkillFinished()
        {
            if (!IsSkillRunning) return;

            var stopRunningByLackHolding =
                skillControllerConfig.HoldingType == SkillHoldingType.RunningDuration
                && !continueRunningWhileHolding;

            continueRunningWhileHolding = false;

            if (executionTimer.Expired(objectContext.Runner) || stopRunningByLackHolding)
            {
                ResetOnFinish();

                listener?.OnSkillFinished(this);
            }
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

            if (castTimer.IsRunning && skillControllerConfig.HoldingType == SkillHoldingType.PowerChargeOnCast)
            {
                var powerChargeProgress = GetPowerChargeProgress();
                listener?.OnPowerChargeProgressChanged(this, true, GetPowerChargeLevel(powerChargeProgress), powerChargeProgress);
            }
        }

        private void CheckCastFinished()
        {
            if (!castTimer.IsRunning) return;

            var powerChargeFinished =
                skillControllerConfig.HoldingType == SkillHoldingType.PowerChargeOnCast
                && !continueRunningWhileHolding;

            if (castTimer.Expired(objectContext.Runner) || powerChargeFinished)
            {
                castTimer = default;
                executionTimer = TickTimer.CreateFromTicks(
                    objectContext.Runner,
                    skillControllerConfig.ExecutionDurationTicks
                );

                if (skillControllerConfig.HoldingType == SkillHoldingType.PowerChargeOnCast)
                {
                    powerChargeLevel = GetPowerChargeLevel(GetPowerChargeProgress());
                    listener?.OnPowerChargeProgressChanged(this, false, 0, 0);
                }

                if (skillControllerConfig.CooldownStartType == SkillCooldownStartType.OnExecute)
                {
                    StartCooldown();
                }

                AddSpawnActions(skillControllerConfig.RunOnExecutionSkillConfigs);
                
                listener?.OnSkillFinishedCasting(this);
            }
        }

        private void StartCooldown()
        {
            cooldownTimer =
                TickTimer.CreateFromTicks(objectContext.Runner, skillControllerConfig.CooldownTicks);
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

        public void CancelActivation()
        {
            if (!isActivating) return;

            ResetOnFinish();

            listener?.OnSkillCanceled(this);
        }

        public bool TryInterrupt(SkillInterruptionType interruptionTypes)
        {
            if (!IsSkillRunning) return false;

            ref var interruptionData = ref castTimer.IsRunning
                ? ref skillControllerConfig.CastInterruptionData
                : ref skillControllerConfig.ExecutionInterruptionData;
            
            var shouldInterrupt = false;
            
            if (interruptionTypes.HasFlag(SkillInterruptionType.OwnerDead))
            {
                shouldInterrupt = true;
            }

            if (interruptionTypes.HasFlag(SkillInterruptionType.OwnerStunned))
            {
                shouldInterrupt = true;
            }

            if (interruptionTypes.HasFlag(SkillInterruptionType.SelectedTargetDead))
            {
                shouldInterrupt |= interruptionData.BySelectedTargetDeath;
            }

            if (interruptionTypes.HasFlag(SkillInterruptionType.AnotherSkillActivation))
            {
                shouldInterrupt |= interruptionData.ByAnotherSkillActivation;
            }

            if (interruptionTypes.HasFlag(SkillInterruptionType.Cancel))
            {
                shouldInterrupt |= interruptionData.ByCancel;
            }
            
            if (!shouldInterrupt) return false;

            if (castTimer.IsRunning && skillControllerConfig.HoldingType == SkillHoldingType.PowerChargeOnCast)
            {
                listener?.OnPowerChargeProgressChanged(this, false, 0, 0);
            }
            
            foreach (var skillComponent in skillComponents)
            {
                skillComponent.TryInterrupt();
            }

            ResetOnFinish();
            
            listener?.OnSkillInterrupted(this);

            return true;
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
            executionTimer = default;
            isActivating = false;
            castStartTick = default;

            selectedUnit = null;
            selectedUnitId = default;
            effectTargetsIdList.Clear();
            initialMapPoint = default;
            dynamicMapPoint = default;
            powerChargeLevel = default;
            continueRunningWhileHolding = default;

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
                    heatLevel: heatLevel,
                    stackCount: stackCount,
                    initialMapPoint: initialMapPoint,
                    dynamicMapPoint: dynamicMapPoint,
                    powerChargeLevel: powerChargeLevel,
                    executionChargeLevel: 0,
                    selfUnitId: objectContext.Id,
                    selectedUnitId: selectedUnitId,
                    targetUnitIdsList: effectTargetsIdList,
                    alliesLayerMask: alliesLayerMask,
                    opponentsLayerMask: opponentsLayerMask,
                    onSpawnNewSkillComponent: OnSpawnNewSkillComponent
                );
                OnSpawnNewSkillComponent(skillComponent);
            }
        }

        private void OnSpawnNewSkillComponent(SkillComponent spawnedSkillComponent)
        {
            spawnedSkillComponent.OnReadyToRelease += OnReadyToReleaseSkillComponent;
            skillComponents.Add(spawnedSkillComponent);
        }

        private void OnReadyToReleaseSkillComponent(SkillComponent skillComponent)
        {
            skillComponent.OnReadyToRelease -= OnReadyToReleaseSkillComponent;
            skillComponents.Remove(skillComponent);
        }

        public interface Listener
        {
            public void OnSkillCooldownChanged(SkillController skill);
            public void OnPowerChargeProgressChanged(SkillController skill, bool isCharging, int powerChargeLevel, int powerChargeProgress);
            public void OnSkillWaitingForPointTarget(SkillController skill);
            public void OnSkillWaitingForUnitTarget(SkillController skill);
            public void OnSkillWaitingForPowerCharge(SkillController skill);
            public void OnSkillStartCasting(SkillController skill);
            public void OnSkillFinishedCasting(SkillController skill);
            public void OnSkillFinished(SkillController skill);
            public void OnSkillCanceled(SkillController skill);
            public void OnSkillInterrupted(SkillController skill);
        }
    }
}