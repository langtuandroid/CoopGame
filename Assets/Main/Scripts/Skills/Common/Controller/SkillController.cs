﻿using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.Resources;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component;
using Main.Scripts.Skills.Common.Component.Config;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Skills.Common.Controller
{
    public class SkillController : GameLoopEntity
    {
        [SerializeField]
        private SkillControllerConfig skillControllerConfig = default!;

        private GameObject? marker;
        private SkillConfigsBank skillConfigsBank = default!;

        [Networked]
        private Vector3 initialMapPoint { get; set; }
        [Networked]
        private Vector3 dynamicMapPoint { get; set; }
        [Networked]
        private NetworkObject? selectedUnit { get; set; }

        [Networked]
        private TickTimer executionTimer { get; set; }
        [Networked]
        private TickTimer castTimer { get; set; }
        [Networked]
        private TickTimer cooldownTimer { get; set; }
        [Networked]
        private bool isActivating { get; set; }

        [Networked]
        [Capacity(10)]
        private NetworkLinkedList<SkillComponent?> skillComponents => default;
        
        private Transform selfUnitTransform = default!;
        private LayerMask alliesLayerMask;
        private LayerMask opponentsLayerMask;
        private PlayerRef ownerRef;

        [HideInInspector]
        public UnityEvent<SkillController> OnWaitingForPointTargetEvent = default!;
        [HideInInspector]
        public UnityEvent<SkillController> OnWaitingForUnitTargetEvent = default!;
        [HideInInspector]
        public UnityEvent<SkillController> OnSkillExecutedEvent = default!;
        [HideInInspector]
        public UnityEvent<SkillController> OnSkillFinishedEvent = default!;
        [HideInInspector]
        public UnityEvent<SkillController> OnSkillCanceledEvent = default!;

        public bool IsSkillExecuting => executionTimer.IsRunning;
        public SkillActivationType ActivationType => skillControllerConfig.ActivationType;
        public UnitTargetType SelectionTargetType => skillControllerConfig.SelectionTargetType;

        private void OnValidate()
        {
            SkillConfigsValidationHelper.Validate(skillControllerConfig);
        }

        public void Init(
            Transform selfUnitTransform,
            LayerMask alliesLayerMask,
            LayerMask opponentsLayerMask
        )
        {
            this.selfUnitTransform = selfUnitTransform;
            this.alliesLayerMask = alliesLayerMask;
            this.opponentsLayerMask = opponentsLayerMask;
        }

        public override void Spawned()
        {
            base.Spawned();
            skillConfigsBank = GlobalResources.Instance.ThrowWhenNull().SkillConfigsBank;
        }

        public override void Render()
        {
            if (!Runner.LocalPlayer == ownerRef) return;

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
                        marker = Instantiate(
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

        public void SetOwnerRef(PlayerRef ownerRef)
        {
            this.ownerRef = ownerRef;

        }

        public bool Activate(PlayerRef owner)
        {
            if (isActivating || IsSkillExecuting || !cooldownTimer.ExpiredOrNotRunning(Runner))
            {
                return false;
            }

            isActivating = true;
            skillComponents.Clear();

            switch (skillControllerConfig.ActivationType)
            {
                case SkillActivationType.Instantly:
                    Execute();
                    break;
                case SkillActivationType.WithMapPointTarget:
                    OnWaitingForPointTargetEvent.Invoke(this);
                    break;
                case SkillActivationType.WithUnitTarget:
                    OnWaitingForUnitTargetEvent.Invoke(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        public void UpdateMapPoint(Vector3 mapPoint)
        {
            dynamicMapPoint = mapPoint;
            if (!IsSkillExecuting)
            {
                initialMapPoint = mapPoint;
            }

            foreach (var skillComponent in skillComponents)
            {
                if (skillComponent != null)
                {
                    skillComponent.UpdateMapPoint(mapPoint);
                }
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

            if (IsSkillExecuting) return;

            isActivating = false;

            executionTimer = TickTimer.CreateFromSeconds(Runner, skillControllerConfig.ExecutionDurationSec);
            castTimer = TickTimer.CreateFromSeconds(Runner, skillControllerConfig.CastDurationSec);

            OnSkillExecutedEvent.Invoke(this);
            
            SpawnSkills(skillControllerConfig.RunOnStartSkillConfigs);

            CheckCastFinished();
        }

        public override void OnBeforePhysicsSteps()
        {
            CheckCastFinished();

            if (executionTimer.Expired(Runner))
            {
                ResetOnFinish();

                OnSkillFinishedEvent.Invoke(this);
            }
        }

        private void CheckCastFinished()
        {
            if (castTimer.Expired(Runner))
            {
                castTimer = default;
                SpawnSkills(skillControllerConfig.RunAfterCastSkillConfigs);
            }
        }

        //todo
        public void OnClickTrigger()
        {
            foreach (var skillComponent in skillComponents)
            {
                if (skillComponent != null)
                {
                    skillComponent.OnClickTrigger();
                }
            }
        }

        public bool TryInterrupt()
        {
            //todo доделать
            foreach (var skillComponent in skillComponents)
            {
                if (skillComponent != null)
                {
                    skillComponent.TryInterrupt();
                }
            }

            return false;
        }

        public void CancelActivation()
        {
            if (!isActivating) return;

            ResetOnFinish();

            OnSkillCanceledEvent.Invoke(this);
        }

        public bool IsDisabledMove()
        {
            var isDisableOnCast = skillControllerConfig.DisableMoveOnCast && !castTimer.ExpiredOrNotRunning(Runner);
            var isDisableOnExecution = skillControllerConfig.DisableMoveOnExecution && IsSkillExecuting;
            return isDisableOnCast || isDisableOnExecution;
        }

        private void ResetOnFinish()
        {
            castTimer = default;
            executionTimer = default;
            isActivating = false;

            selectedUnit = null;
            initialMapPoint = default;
            dynamicMapPoint = default;

            foreach (var skillComponent in skillComponents)
            {
                if (skillComponent != null)
                {
                    skillComponent.OnLostControl();
                }
            }
            skillComponents.Clear();
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

                var spawnedSkill = Runner.Spawn(
                    prefab: skillConfig.Prefab,
                    position: position,
                    rotation: rotation,
                    onBeforeSpawned: (_, skillObject) =>
                    {
                        skillObject.GetComponent<SkillComponent>().Init(
                            skillConfigId: skillConfigsBank.GetSkillConfigId(skillConfig),
                            ownerId: ownerRef,
                            initialMapPoint: initialMapPoint,
                            dynamicMapPoint: dynamicMapPoint,
                            selfUnit: Object,
                            selectedUnit: selectedUnit,
                            alliesLayerMask: alliesLayerMask,
                            opponentsLayerMask: opponentsLayerMask,
                            onSpawnNewSkillComponent: spawnedSkillComponent =>
                            {
                                skillComponents.Add(spawnedSkillComponent);
                            } 
                        );
                    }
                );
                if (spawnedSkill != null)
                {
                    skillComponents.Add(spawnedSkill);
                }
            }
        }
    }
}