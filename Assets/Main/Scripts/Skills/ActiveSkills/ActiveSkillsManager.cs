using System;
using System.Linq;
using Fusion;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Controller;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Skills.ActiveSkills
{
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

        [HideInInspector]
        public UnityEvent<ActiveSkillType, ActiveSkillState> OnActiveSkillStateChangedEvent = default!;

        private void Awake()
        {
            if (primarySkill != null)
            {
                primarySkill.Init(
                    transform,
                    alliesLayerMask,
                    opponentsLayerMask
                );
            }

            if (dashSkill != null)
            {
                dashSkill.Init(
                    transform,
                    alliesLayerMask,
                    opponentsLayerMask
                );
            }

            if (firstSkill != null)
            {
                firstSkill.Init(
                    transform,
                    alliesLayerMask,
                    opponentsLayerMask
                );
            }

            if (secondSkill != null)
            {
                secondSkill.Init(
                    transform,
                    alliesLayerMask,
                    opponentsLayerMask
                );
            }

            if (thirdSkill != null)
            {
                thirdSkill.Init(
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
            return skill.Activate(Object.InputAuthority);
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