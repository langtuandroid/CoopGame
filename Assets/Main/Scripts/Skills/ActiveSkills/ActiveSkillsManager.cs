using System;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Skills.ActiveSkills
{
    public class ActiveSkillsManager : NetworkBehaviour
    {
        [SerializeField]
        private ActiveSkillBase primarySkill = default!;
        [SerializeField]
        private ActiveSkillBase secondarySkill = default!;
        [SerializeField]
        private ActiveSkillBase dashSkill = default!;

        [HideInInspector]
        [Networked] 
        public ActiveSkillState CurrentSkillState { get; private set; }
        [Networked]
        private ActiveSkillType currentSkillType { get; set; }
        [Networked]
        private Vector2 targetMapPosition { get; set; }

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

                if (skill is ActivePointSkillBase pointSkill)
                {
                    pointSkill.OnWaitingForPointEvent.AddListener(OnActiveSkillWaitingForPoint);
                    pointSkill.OnSkillCanceledEvent.AddListener(OnActiveSkillCanceled);
                }
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
                if (skill is ActivePointSkillBase pointSkill)
                {
                    pointSkill.OnWaitingForPointEvent.RemoveListener(OnActiveSkillWaitingForPoint);
                    pointSkill.OnSkillCanceledEvent.RemoveListener(OnActiveSkillCanceled);
                }
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
            if (skill == null
                || (CurrentSkillState != ActiveSkillState.WaitingForPoint &&
                    CurrentSkillState != ActiveSkillState.WaitingForTarget)
                || skill is not ActivePointSkillBase pointSkill)
            {
                Debug.LogError("Incorrect skill state");
                return;
            }

            pointSkill.Execute();
        }

        public void CancelCurrentSkill()
        {
            var skill = getSkillByType(currentSkillType);
            if (skill == null
                || (CurrentSkillState != ActiveSkillState.WaitingForPoint &&
                    CurrentSkillState != ActiveSkillState.WaitingForTarget)
                || skill is not ActivePointSkillBase pointSkill)
            {
                Debug.LogError("Incorrect skill state");
                return;
            }

            pointSkill.Cancel();
        }

        public void ApplyTargetMapPosition(Vector2 targetMapPosition)
        {
            this.targetMapPosition = targetMapPosition;
            var skill = getSkillByType(currentSkillType);
            if (skill == null || skill is not ActivePointSkillBase pointSkill)
            {
                return;
            }
            pointSkill.ApplyTargetPosition(targetMapPosition);
        }

        public bool IsCurrentSkillOverrideMove()
        {
            return getSkillByType(currentSkillType)?.IsOverrideMove() ?? false;
        }

        private void UpdateSkillState(ActiveSkillState skillState)
        {
            CurrentSkillState = skillState;
            OnActiveSkillStateChangedEvent.Invoke(currentSkillType, CurrentSkillState);
        }

        private void OnActiveSkillWaitingForPoint(ActiveSkillBase skill)
        {
            UpdateSkillState(ActiveSkillState.WaitingForPoint);
            ApplyTargetMapPosition(targetMapPosition);
        }

        private void OnActiveSkillExecuted(ActiveSkillBase skill)
        {
            UpdateSkillState(ActiveSkillState.Attacking);
        }

        private void OnActiveSkillFinished(ActiveSkillBase skill)
        {
            UpdateSkillState(ActiveSkillState.Finished);
            currentSkillType = ActiveSkillType.None;
            CurrentSkillState = ActiveSkillState.NotAttacking;
        }

        private void OnActiveSkillCanceled(ActiveSkillBase skill)
        {
            UpdateSkillState(ActiveSkillState.Canceled);
            currentSkillType = ActiveSkillType.None;
            CurrentSkillState = ActiveSkillState.NotAttacking;
        }

        private ActiveSkillBase? getSkillByType(ActiveSkillType skillType)
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