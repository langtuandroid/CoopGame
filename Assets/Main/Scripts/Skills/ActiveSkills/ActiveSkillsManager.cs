using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Controller;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Skills.ActiveSkills
{
    [SimulationBehaviour(
        Stages = (SimulationStages) 8,
        Modes  = (SimulationModes) 8
    )]
    public class ActiveSkillsManager : NetworkBehaviour
    {
        [SerializeField]
        private LayerMask alliesLayerMask;
        [SerializeField]
        private LayerMask opponentsLayerMask;
        
        [SerializeField]
        private SkillController? primarySkill;
        [SerializeField]
        private SkillController? dashSkill;
        [SerializeField]
        private SkillController? firstSkill;
        [SerializeField]
        private SkillController? secondSkill;
        [SerializeField]
        private SkillController? thirdSkill;

        [Networked]
        [HideInInspector]
        public ActiveSkillState CurrentSkillState { get; private set; }
        [Networked]
        private ActiveSkillType currentSkillType { get; set; }
        [Networked]
        private Vector3 targetMapPosition { get; set; }
        [Networked]
        private NetworkId unitTargetId { get; set; }

        private List<SkillController> allSkillControllers = new();
        private PlayerRef owner;

        [HideInInspector]
        public UnityEvent<ActiveSkillType, ActiveSkillState> OnActiveSkillStateChangedEvent = default!;

        private void Awake()
        {
            if (primarySkill != null)
            {
                allSkillControllers.Add(primarySkill);
            }

            if (dashSkill != null)
            {
                allSkillControllers.Add(dashSkill);
            }

            if (firstSkill != null)
            {
                allSkillControllers.Add(firstSkill);
            }

            if (secondSkill != null)
            {
                allSkillControllers.Add(secondSkill);
            }

            if (thirdSkill != null)
            {
                allSkillControllers.Add(thirdSkill);
            }

            foreach (var skillController in allSkillControllers)
            {
                skillController.Init(
                    transform,
                    alliesLayerMask,
                    opponentsLayerMask
                );
            }
        }

        public override void Spawned()
        {
            foreach (var skillType in Enum.GetValues(typeof(ActiveSkillType)).Cast<ActiveSkillType>())
            {
                var skill = getSkillByType(skillType);
                if (skill == null)
                {
                    continue;
                }

                skill.OnSkillFinishedEvent.AddListener(OnActiveSkillFinished);
                skill.OnSkillExecutedEvent.AddListener(OnActiveSkillExecuted);
                
                skill.OnWaitingForPointTargetEvent.AddListener(OnActiveSkillWaitingForPointTarget);
                skill.OnWaitingForUnitTargetEvent.AddListener(OnActiveSkillWaitingForUnitTarget);
                skill.OnSkillCanceledEvent.AddListener(OnActiveSkillCanceled);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            foreach (var skillType in Enum.GetValues(typeof(ActiveSkillType)).Cast<ActiveSkillType>())
            {
                var skill = getSkillByType(skillType);
                if (skill == null)
                {
                    continue;
                }

                skill.OnSkillFinishedEvent.RemoveListener(OnActiveSkillFinished);
                skill.OnSkillExecutedEvent.RemoveListener(OnActiveSkillExecuted);
                skill.OnWaitingForPointTargetEvent.RemoveListener(OnActiveSkillWaitingForPointTarget);
                skill.OnSkillCanceledEvent.RemoveListener(OnActiveSkillCanceled);
            }
        }

        public void SetOwner(PlayerRef owner)
        {
            this.owner = owner;
            foreach (var skillController in allSkillControllers)
            {
                skillController.SetOwner(owner);
            }
        }

        public bool ActivateSkill(ActiveSkillType skillType)
        {
            if (CurrentSkillState != ActiveSkillState.NotAttacking)
            {
                return false;
            }

            var skill = getSkillByType(skillType);
            if (skill == null)
            {
                return false;
            }

            currentSkillType = skillType;
            return skill.Activate(owner);
        }

        public void ExecuteCurrentSkill()
        {
            var skill = getSkillByType(currentSkillType);
            if (skill == null || (CurrentSkillState != ActiveSkillState.WaitingForPoint &&
                                  CurrentSkillState != ActiveSkillState.WaitingForTarget))
            {
                Debug.LogError("Incorrect skill state");
                return;
            }

            if (CurrentSkillState == ActiveSkillState.WaitingForTarget && Runner.FindObject(unitTargetId) == null)
            {
                skill.CancelActivation();
                return;
            }

            skill.Execute();
        }

        public void CancelCurrentSkill()
        {
            var skill = getSkillByType(currentSkillType);
            if (skill == null || (CurrentSkillState != ActiveSkillState.WaitingForPoint &&
                                  CurrentSkillState != ActiveSkillState.WaitingForTarget))
            {
                Debug.LogError("Incorrect skill state");
                return;
            }

            skill.CancelActivation();
        }

        public void ApplyTargetMapPosition(Vector3 targetMapPosition)
        {
            this.targetMapPosition = targetMapPosition;
            foreach (var skillType in Enum.GetValues(typeof(ActiveSkillType)).Cast<ActiveSkillType>())
            {
                var skill = getSkillByType(skillType);
                if (skill == null)
                {
                    continue;
                }

                skill.UpdateMapPoint(targetMapPosition);
            }
        }

        public void ApplyUnitTarget(NetworkId unitTargetId)
        {
            this.unitTargetId = unitTargetId;
            foreach (var skillType in Enum.GetValues(typeof(ActiveSkillType)).Cast<ActiveSkillType>())
            {
                var skill = getSkillByType(skillType);
                if (skill == null)
                {
                    continue;
                }

                var unitTarget = Runner.FindObject(unitTargetId);
                if (unitTarget != null)
                {
                    skill.ApplyUnitTarget(unitTarget);
                }
            }
        }

        public UnitTargetType GetSelectionTargetType()
        {
            var skill = getSkillByType(currentSkillType);
            if (skill == null || (CurrentSkillState != ActiveSkillState.WaitingForPoint &&
                                  CurrentSkillState != ActiveSkillState.WaitingForTarget))
            {
                throw new Exception("Incorrect skill state");
            }

            return skill.SelectionTargetType;
        }

        public bool IsCurrentSkillDisableMove()
        {
            return getSkillByType(currentSkillType)?.IsDisabledMove() ?? false;
        }

        private void UpdateSkillState(ActiveSkillState skillState)
        {
            CurrentSkillState = skillState;
            OnActiveSkillStateChangedEvent.Invoke(currentSkillType, CurrentSkillState);
        }

        private void OnActiveSkillWaitingForPointTarget(SkillController skill)
        {
            UpdateSkillState(ActiveSkillState.WaitingForPoint);
            ApplyTargetMapPosition(targetMapPosition);
        }

        private void OnActiveSkillWaitingForUnitTarget(SkillController skill)
        {
            UpdateSkillState(ActiveSkillState.WaitingForTarget);
            ApplyUnitTarget(unitTargetId);
        }

        private void OnActiveSkillExecuted(SkillController skill)
        {
            UpdateSkillState(ActiveSkillState.Attacking);
        }

        private void OnActiveSkillFinished(SkillController skill)
        {
            UpdateSkillState(ActiveSkillState.Finished);
            currentSkillType = ActiveSkillType.NONE;
            CurrentSkillState = ActiveSkillState.NotAttacking;
        }

        private void OnActiveSkillCanceled(SkillController skill)
        {
            UpdateSkillState(ActiveSkillState.Canceled);
            currentSkillType = ActiveSkillType.NONE;
            CurrentSkillState = ActiveSkillState.NotAttacking;
        }

        private SkillController? getSkillByType(ActiveSkillType skillType)
        {
            return skillType switch
            {
                ActiveSkillType.PRIMARY => primarySkill,
                ActiveSkillType.DASH => dashSkill,
                ActiveSkillType.FIRST_SKILL => firstSkill,
                ActiveSkillType.SECOND_SKILL => secondSkill,
                ActiveSkillType.THIRD_SKILL => thirdSkill,
                ActiveSkillType.NONE => null,
                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null)
            };
        }
    }
}