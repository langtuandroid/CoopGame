using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Controller;
using UnityEngine;

namespace Main.Scripts.Skills.ActiveSkills
{
    public class ActiveSkillsManager : SkillController.Listener
    {
        private ActiveSkillsConfig config;
        private DataHolder dataHolder;
        private EventListener eventListener;
        private NetworkObject objectContext = default!;

        private List<SkillController> allSkillControllers = new();
        private PlayerRef ownerRef;

        public ActiveSkillsManager(
            ref ActiveSkillsConfig config,
            DataHolder dataHolder,
            EventListener eventListener,
            Transform transform
        )
        {
            this.config = config;
            this.dataHolder = dataHolder;
            this.eventListener = eventListener;

            if (config.PrimarySkill != null)
            {
                allSkillControllers.Add(config.PrimarySkill);
            }

            if (config.DashSkill != null)
            {
                allSkillControllers.Add(config.DashSkill);
            }

            if (config.FirstSkill != null)
            {
                allSkillControllers.Add(config.FirstSkill);
            }

            if (config.SecondSkill != null)
            {
                allSkillControllers.Add(config.SecondSkill);
            }

            if (config.ThirdSkill != null)
            {
                allSkillControllers.Add(config.ThirdSkill);
            }

            foreach (var skillController in allSkillControllers)
            {
                skillController.Init(
                    transform,
                    config.AlliesLayerMask,
                    config.OpponentsLayerMask
                );
            }
        }

        public void Spawned(NetworkObject objectContext)
        {
            this.objectContext = objectContext;

            foreach (var skillType in Enum.GetValues(typeof(ActiveSkillType)).Cast<ActiveSkillType>())
            {
                var skill = getSkillByType(skillType);
                if (skill == null)
                {
                    continue;
                }

                skill.AddListener(this);
            }
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            foreach (var skillType in Enum.GetValues(typeof(ActiveSkillType)).Cast<ActiveSkillType>())
            {
                var skill = getSkillByType(skillType);
                if (skill == null)
                {
                    continue;
                }

                skill.RemoveListener();
            }

            objectContext = default!;
        }

        public bool ActivateSkill(ActiveSkillType skillType)
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            if (data.currentSkillState != ActiveSkillState.NotAttacking)
            {
                return false;
            }

            var skill = getSkillByType(skillType);
            if (skill == null)
            {
                return false;
            }

            data.currentSkillType = skillType;
            return skill.Activate();
        }

        public void ExecuteCurrentSkill()
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            var skill = getSkillByType(data.currentSkillType);
            if (skill == null || (data.currentSkillState != ActiveSkillState.WaitingForPoint &&
                                  data.currentSkillState != ActiveSkillState.WaitingForTarget))
            {
                Debug.LogError("Incorrect skill state");
                return;
            }

            if (data.currentSkillState == ActiveSkillState.WaitingForTarget &&
                objectContext.Runner.FindObject(data.unitTargetId) == null)
            {
                skill.CancelActivation();
                return;
            }

            skill.Execute();
        }

        public void CancelCurrentSkill()
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            var skill = getSkillByType(data.currentSkillType);
            if (skill == null || (data.currentSkillState != ActiveSkillState.WaitingForPoint &&
                                  data.currentSkillState != ActiveSkillState.WaitingForTarget))
            {
                Debug.LogError("Incorrect skill state");
                return;
            }

            skill.CancelActivation();
        }

        public void ApplyTargetMapPosition(Vector3 targetMapPosition)
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            data.targetMapPosition = targetMapPosition;
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
            ref var data = ref dataHolder.GetActiveSkillsData();

            data.unitTargetId = unitTargetId;
            foreach (var skillType in Enum.GetValues(typeof(ActiveSkillType)).Cast<ActiveSkillType>())
            {
                var skill = getSkillByType(skillType);
                if (skill == null)
                {
                    continue;
                }

                var unitTarget = objectContext.Runner.FindObject(unitTargetId);
                if (unitTarget != null)
                {
                    skill.ApplyUnitTarget(unitTarget);
                }
            }
        }

        public UnitTargetType GetSelectionTargetType()
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            var skill = getSkillByType(data.currentSkillType);
            if (skill == null || (data.currentSkillState != ActiveSkillState.WaitingForPoint &&
                                  data.currentSkillState != ActiveSkillState.WaitingForTarget))
            {
                throw new Exception("Incorrect skill state");
            }

            return skill.SelectionTargetType;
        }

        public bool IsCurrentSkillDisableMove()
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            return getSkillByType(data.currentSkillType)?.IsDisabledMove() ?? false;
        }

        public ActiveSkillState GetCurrentSkillState()
        {
            ref var data = ref dataHolder.GetActiveSkillsData();
            return data.currentSkillState;
        }

        public int GetSkillCooldownLeftTicks(ActiveSkillType skillType)
        {
            return getSkillByType(skillType)?.GetCooldownLeftTicks() ?? 0;
        }

        private void UpdateSkillState(ref ActiveSkillsData data, ActiveSkillState skillState)
        {
            data.currentSkillState = skillState;
            eventListener.OnActiveSkillStateChanged(data.currentSkillType, data.currentSkillState);
        }

        public void OnSkillWaitingForPointTarget(SkillController skill)
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            UpdateSkillState(ref data, ActiveSkillState.WaitingForPoint);
            ApplyTargetMapPosition(data.targetMapPosition);
        }

        public void OnSkillCooldownChanged(SkillController skill)
        {
            var type = getTypeBySkill(skill);
            var cooldownLeftTicks = skill.GetCooldownLeftTicks();
            eventListener.OnSkillCooldownChanged(type, cooldownLeftTicks);
        }

        public void OnSkillWaitingForUnitTarget(SkillController skill)
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            UpdateSkillState(ref data, ActiveSkillState.WaitingForTarget);
            ApplyUnitTarget(data.unitTargetId);
        }

        public void OnSkillExecuted(SkillController skill)
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            UpdateSkillState(ref data, ActiveSkillState.Attacking);
        }

        public void OnSkillFinished(SkillController skill)
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            UpdateSkillState(ref data, ActiveSkillState.Finished);
            data.currentSkillType = ActiveSkillType.NONE;
            data.currentSkillState = ActiveSkillState.NotAttacking;
        }

        public void OnSkillCanceled(SkillController skill)
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            UpdateSkillState(ref data, ActiveSkillState.Canceled);
            data.currentSkillType = ActiveSkillType.NONE;
            data.currentSkillState = ActiveSkillState.NotAttacking;
        }

        private SkillController? getSkillByType(ActiveSkillType skillType)
        {
            return skillType switch
            {
                ActiveSkillType.PRIMARY => config.PrimarySkill,
                ActiveSkillType.DASH => config.DashSkill,
                ActiveSkillType.FIRST_SKILL => config.FirstSkill,
                ActiveSkillType.SECOND_SKILL => config.SecondSkill,
                ActiveSkillType.THIRD_SKILL => config.ThirdSkill,
                ActiveSkillType.NONE => null,
                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null)
            };
        }

        private ActiveSkillType getTypeBySkill(SkillController skill)
        {
            return skill switch
            {
                _ when skill == config.PrimarySkill => ActiveSkillType.PRIMARY,
                _ when skill == config.DashSkill => ActiveSkillType.DASH,
                _ when skill == config.FirstSkill => ActiveSkillType.FIRST_SKILL,
                _ when skill == config.SecondSkill => ActiveSkillType.SECOND_SKILL,
                _ when skill == config.ThirdSkill => ActiveSkillType.THIRD_SKILL,
                _ => throw new Exception("Couldn't get type by skill")
            };
        }

        public interface DataHolder
        {
            public ref ActiveSkillsData GetActiveSkillsData();
        }

        public interface EventListener
        {
            public void OnActiveSkillStateChanged(ActiveSkillType type, ActiveSkillState state);
            public void OnSkillCooldownChanged(ActiveSkillType type, int cooldownLeftTicks) { }
        }
    }
}