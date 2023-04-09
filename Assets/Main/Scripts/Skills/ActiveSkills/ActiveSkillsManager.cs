using System;
using System.Linq;
using Fusion;
using Main.Scripts.Skills.Common.Controller;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Skills.ActiveSkills
{
    public class ActiveSkillsManager : NetworkBehaviour
    {
        [SerializeField]
        private SkillController primarySkill = default!;
        [SerializeField]
        private SkillController secondarySkill = default!;
        [SerializeField]
        private SkillController dashSkill = default!;

        [HideInInspector]
        [Networked] 
        public ActiveSkillState CurrentSkillState { get; private set; }
        [Networked]
        private ActiveSkillType currentSkillType { get; set; }
        [Networked]
        private Vector3 targetMapPosition { get; set; }

        [HideInInspector]
        public UnityEvent<ActiveSkillType, ActiveSkillState> OnActiveSkillStateChangedEvent = default!;

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

        private void OnActiveSkillExecuted(SkillController skill)
        {
            UpdateSkillState(ActiveSkillState.Attacking);
        }

        private void OnActiveSkillFinished(SkillController skill)
        {
            UpdateSkillState(ActiveSkillState.Finished);
            currentSkillType = ActiveSkillType.None;
            CurrentSkillState = ActiveSkillState.NotAttacking;
        }

        private void OnActiveSkillCanceled(SkillController skill)
        {
            UpdateSkillState(ActiveSkillState.Canceled);
            currentSkillType = ActiveSkillType.None;
            CurrentSkillState = ActiveSkillState.NotAttacking;
        }

        private SkillController? getSkillByType(ActiveSkillType skillType)
        {
            return skillType switch
            {
                ActiveSkillType.Primary => primarySkill,
                ActiveSkillType.SecondarySkill => secondarySkill,
                ActiveSkillType.Dash => dashSkill,
                ActiveSkillType.None => null,
                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null)
            };
        }
    }
}