using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Core.Architecture;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Charge;
using Main.Scripts.Skills.Common.Controller;
using UnityEngine;

namespace Main.Scripts.Skills.ActiveSkills
{
    public class ActiveSkillsManager : SkillController.Listener
    {
        private ActiveSkillsConfig config;
        private DataHolder dataHolder;
        private EventListener eventListener;
        private Transform transform;
        private SkillHeatLevelManager skillHeatLevelManager = null!;
        private NetworkObject objectContext = null!;

        private Dictionary<ActiveSkillType, SkillController> skillControllersMap = new();
        private Dictionary<SkillController, ActiveSkillType> skillTypesMap = new();
        private PlayerRef ownerRef;

        private ActivateSkillActionData activateSkillActionData;
        private bool shouldExecuteCurrentSkill;
        private bool shouldCancelCurrentSkill;

        public ActiveSkillsManager(
            DataHolder dataHolder,
            EventListener eventListener,
            Transform transform
        )
        {
            this.dataHolder = dataHolder;
            this.eventListener = eventListener;
            this.transform = transform;
        }

        public static void OnValidate(ref ActiveSkillsConfig activeSkillsConfig)
        {
            if (activeSkillsConfig.PrimarySkillConfig != null)
            {
                SkillConfigsValidationHelper.Validate(activeSkillsConfig.PrimarySkillConfig);
            }
            if (activeSkillsConfig.DashSkillConfig != null)
            {
                SkillConfigsValidationHelper.Validate(activeSkillsConfig.DashSkillConfig);
            }
            if (activeSkillsConfig.FirstSkillConfig != null)
            {
                SkillConfigsValidationHelper.Validate(activeSkillsConfig.FirstSkillConfig);
            }
            if (activeSkillsConfig.SecondSkillConfig != null)
            {
                SkillConfigsValidationHelper.Validate(activeSkillsConfig.SecondSkillConfig);
            }
            if (activeSkillsConfig.ThirdSkillConfig != null)
            {
                SkillConfigsValidationHelper.Validate(activeSkillsConfig.ThirdSkillConfig);
            }
        }

        public void Spawned(
            NetworkObject objectContext,
            bool isPlayerOwner,
            ref ActiveSkillsConfig config
        )
        {
            this.objectContext = objectContext;
            this.config = config;
            skillHeatLevelManager = dataHolder.GetCachedComponent<SkillHeatLevelManager>();

            InitSkillControllers();

            activateSkillActionData = default;
            shouldExecuteCurrentSkill = false;
            shouldCancelCurrentSkill = false;

            foreach (var (_, skillController) in skillControllersMap)
            {
                skillController.SetListener(this);
                skillController.Spawned(objectContext, isPlayerOwner);
            }
        }
        
        private void InitSkillControllers()
        {
            if (config.PrimarySkillConfig != null)
            {
                InitSkillController(transform, ActiveSkillType.PRIMARY, config.PrimarySkillConfig);
            }

            if (config.DashSkillConfig != null)
            {
                InitSkillController(transform, ActiveSkillType.DASH, config.DashSkillConfig);
            }

            if (config.FirstSkillConfig != null)
            {
                InitSkillController(transform, ActiveSkillType.FIRST_SKILL, config.FirstSkillConfig);
            }

            if (config.SecondSkillConfig != null)
            {
                InitSkillController(transform, ActiveSkillType.SECOND_SKILL, config.SecondSkillConfig);
            }

            if (config.ThirdSkillConfig != null)
            {
                InitSkillController(transform, ActiveSkillType.THIRD_SKILL, config.ThirdSkillConfig);
            }
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            foreach (var (_, skillController) in skillControllersMap)
            {
                skillController.SetListener(null);
                skillController.Despawned();
                skillController.Release();
            }

            objectContext = null!;
            skillHeatLevelManager = null!;
        }

        public void Render()
        {
            foreach (var (_, skillController) in skillControllersMap)
            {
                skillController.Render();
            }
        }

        public void AddActivateSkill(ActiveSkillType skillType, bool shouldExecute)
        {
            activateSkillActionData = new ActivateSkillActionData
            {
                SkillType = skillType,
                ShouldExecute = shouldExecute
            };
        }

        public void AddExecuteCurrentSkill()
        {
            shouldExecuteCurrentSkill = true;
        }

        public void AddCancelCurrentSkill()
        {
            shouldCancelCurrentSkill = true;
        }

        public void ApplyHolding(ActiveSkillType skillType)
        {
            var currentSkillType = GetCurrentSkillType();
            if (currentSkillType != skillType || GetCurrentSkillState() != ActiveSkillState.Attacking) return;

            var skillController = GetSkillByType(skillType);
            skillController?.ApplyHolding();
        }

        public void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.SkillActivationPhase:
                    CancelCurrentSkill();
                    ActivateSkill();
                    ExecuteCurrentSkill();
                    break;
                case GameLoopPhase.SkillCheckCastFinished:
                case GameLoopPhase.SkillSpawnPhase:
                case GameLoopPhase.SkillUpdatePhase:
                case GameLoopPhase.VisualStateUpdatePhase:
                    foreach (var (_, skillController) in skillControllersMap)
                    {
                        skillController.OnGameLoopPhase(phase);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        private void InitSkillController(
            Transform transform,
            ActiveSkillType skillType,
            SkillControllerConfig skillControllerConfig
        )
        {
            var skillController = new SkillController();
            skillController.Init(
                skillControllerConfig: skillControllerConfig,
                selfUnitTransform: transform,
                alliesLayerMask: config.AlliesLayerMask,
                opponentsLayerMask: config.OpponentsLayerMask
            );
            skillControllersMap[skillType] = skillController;
            skillTypesMap[skillController] = skillType;
        }

        private void ActivateSkill()
        {
            var skillType = activateSkillActionData.SkillType;
            var shouldExecute = activateSkillActionData.ShouldExecute;
            activateSkillActionData = default;

            if (skillType == ActiveSkillType.NONE) return;

            ref var data = ref dataHolder.GetActiveSkillsData();

            if (data.currentSkillState != ActiveSkillState.NotAttacking)
            {
                return;
            }

            var skill = GetSkillByType(skillType);
            if (skill == null)
            {
                return;
            }

            data.currentSkillType = skillType;
            skill.Activate(skillHeatLevelManager.HeatLevel, 0);
            var skillState = GetCurrentSkillState();
            if (shouldExecute
                && skillState is ActiveSkillState.WaitingForTarget or ActiveSkillState.WaitingForPoint)
            {
                shouldExecuteCurrentSkill = shouldExecute;
            }
        }

        private void ExecuteCurrentSkill()
        {
            if (!shouldExecuteCurrentSkill) return;
            shouldExecuteCurrentSkill = false;
            
            ref var data = ref dataHolder.GetActiveSkillsData();

            var skill = GetSkillByType(data.currentSkillType);
            if (skill == null || data.currentSkillState
                    is not ActiveSkillState.WaitingForPoint
                    and not ActiveSkillState.WaitingForTarget
                    and not ActiveSkillState.WaitingForPowerCharge)
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

        private void CancelCurrentSkill()
        {
            if (!shouldCancelCurrentSkill) return;
            shouldCancelCurrentSkill = false;
            
            ref var data = ref dataHolder.GetActiveSkillsData();

            var skill = GetSkillByType(data.currentSkillType);
            if (skill == null || data.currentSkillState
                    is not ActiveSkillState.WaitingForPoint
                    and not ActiveSkillState.WaitingForTarget
                    and not ActiveSkillState.WaitingForPowerCharge)
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
                var skill = GetSkillByType(skillType);
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
                var skill = GetSkillByType(skillType);
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

            var skill = GetSkillByType(data.currentSkillType);
            if (skill == null || data.currentSkillState
                    is not ActiveSkillState.WaitingForPoint
                    and not ActiveSkillState.WaitingForTarget
                    and not ActiveSkillState.WaitingForPowerCharge)
            {
                throw new Exception("Incorrect skill state");
            }

            return skill.SelectionTargetType;
        }

        public bool IsCurrentSkillDisableMove()
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            return GetSkillByType(data.currentSkillType)?.IsDisabledMove() ?? false;
        }

        public ActiveSkillState GetCurrentSkillState()
        {
            ref var data = ref dataHolder.GetActiveSkillsData();
            return data.currentSkillState;
        }

        public ActiveSkillType GetCurrentSkillType()
        {
            ref var data = ref dataHolder.GetActiveSkillsData();
            return data.currentSkillType;
        }

        public int GetSkillCooldownLeftTicks(ActiveSkillType skillType)
        {
            return GetSkillByType(skillType)?.GetCooldownLeftTicks() ?? 0;
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
            var type = GetTypeBySkill(skill);
            var cooldownLeftTicks = skill.GetCooldownLeftTicks();
            eventListener.OnSkillCooldownChanged(type, cooldownLeftTicks);
        }

        public void OnPowerChargeProgressChanged(
            SkillController skill,
            bool isCharging,
            int powerChargeLevel,
            int powerChargeProgress
        )
        {
            eventListener.OnPowerChargeProgressChanged(
                GetTypeBySkill(skill),
                isCharging,
                powerChargeLevel,
                powerChargeProgress
            );
        }

        public void OnSkillWaitingForUnitTarget(SkillController skill)
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            UpdateSkillState(ref data, ActiveSkillState.WaitingForTarget);
            ApplyUnitTarget(data.unitTargetId);
        }

        public void OnSkillWaitingForPowerCharge(SkillController skill)
        {
            ref var data = ref dataHolder.GetActiveSkillsData();
            
            UpdateSkillState(ref data, ActiveSkillState.WaitingForPowerCharge);
        }

        public void OnSkillStartCasting(SkillController skill)
        {
            ref var data = ref dataHolder.GetActiveSkillsData();

            UpdateSkillState(ref data, ActiveSkillState.Casting);
        }

        public void OnSkillFinishedCasting(SkillController skill)
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

        private SkillController? GetSkillByType(ActiveSkillType skillType)
        {
            return skillControllersMap.GetValueOrDefault(skillType);
        }

        private ActiveSkillType GetTypeBySkill(SkillController skill)
        {
            return skillTypesMap[skill];
        }

        private struct ActivateSkillActionData
        {
            public ActiveSkillType SkillType;
            public bool ShouldExecute;
        }

        public interface DataHolder : ComponentsHolder
        {
            public ref ActiveSkillsData GetActiveSkillsData();
        }

        public interface EventListener
        {
            public void OnActiveSkillStateChanged(ActiveSkillType type, ActiveSkillState state);
            public void OnSkillCooldownChanged(ActiveSkillType type, int cooldownLeftTicks) { }

            public void OnPowerChargeProgressChanged(
                ActiveSkillType type,
                bool isCharging,
                int powerChargeLevel,
                int powerChargeProgress
            ) { }
        }
    }
}