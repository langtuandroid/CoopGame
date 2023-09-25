using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Skills.Common.Controller;
using Main.Scripts.Skills.PassiveSkills.Triggers;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills
{
    public class PassiveSkillsManager
    {
        private PassiveSkillsConfig config;
        private Affectable affectable;

        private Dictionary<PassiveSkillTriggerType, List<PassiveSkillController>> skillControllersListMap = new();

        private List<KeyValuePair<SkillController, NetworkObject?>> activateSkillActions = new();

        public PassiveSkillsManager(
            ref PassiveSkillsConfig config,
            Affectable affectable,
            Transform transform
        )
        {
            this.config = config;
            this.affectable = affectable;

            foreach (var type in Enum.GetValues(typeof(PassiveSkillTriggerType)).Cast<PassiveSkillTriggerType>())
            {
                skillControllersListMap.Add(type, new List<PassiveSkillController>());
            }

            foreach (var passiveSkillControllerData in config.PassiveSkillControllersDataList)
            {
                var skillController = new PassiveSkillController(
                    passiveSkillTrigger: passiveSkillControllerData.PassiveSkillTrigger,
                    skillControllerConfig: passiveSkillControllerData.SkillControllerConfig,
                    selfUnitTransform: transform,
                    alliesLayerMask: config.AlliesLayerMask,
                    opponentsLayerMask: config.OpponentsLayerMask
                );

                var type = passiveSkillControllerData.PassiveSkillTrigger switch
                {
                    SpawnPassiveSkillTrigger => PassiveSkillTriggerType.OnSpawn,
                    DeadPassiveSkillTrigger => PassiveSkillTriggerType.OnDead,
                    DamagePassiveSkillTrigger => PassiveSkillTriggerType.OnTakenDamage,
                    HealPassiveSkillTrigger => PassiveSkillTriggerType.OnTakenHeal,
                    _ => throw new ArgumentOutOfRangeException(nameof(passiveSkillControllerData.PassiveSkillTrigger),
                        passiveSkillControllerData.PassiveSkillTrigger, null)
                };

                skillControllersListMap[type].Add(skillController);
            }
        }

        public static void OnValidate(GameObject gameObject, ref PassiveSkillsConfig config)
        {
            if (config.InitialEffects.Any(effectsCombination => effectsCombination == null))
            {
                throw new ArgumentNullException(
                    $"{gameObject.name}: has empty value in PassiveSkillsConfig::InitialEffects");
            }

            if (config.PassiveSkillControllersDataList.Any(passiveSkillControllerData => 
                    passiveSkillControllerData.SkillControllerConfig == null || passiveSkillControllerData.PassiveSkillTrigger == null))
            {
                throw new ArgumentNullException(
                    $"{gameObject.name}: has empty value in PassiveSkillsConfig::PassiveSkillControllersDataList");
            }
        }

        public void Spawned(NetworkObject objectContext)
        {
            foreach (var (_, skillControllersList) in skillControllersListMap)
            {
                foreach (var skillController in skillControllersList)
                {
                    skillController.Spawned(objectContext);
                }
            }
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            foreach (var (_, skillControllersList) in skillControllersListMap)
            {
                foreach (var skillController in skillControllersList)
                {
                    skillController.Despawned(runner, hasState);
                }
            }
        }

        public void Init()
        {
            activateSkillActions.Clear();
            
            foreach (var effectsCombination in config.InitialEffects)
            {
                affectable.AddEffects(effectsCombination);
            }
        }

        public void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.SkillActivationPhase:
                    foreach (var (skillController, selectedTarget) in activateSkillActions)
                    {
                        ActivateSkill(skillController, selectedTarget);
                    }
                    activateSkillActions.Clear();
                    break;
                case GameLoopPhase.SkillUpdatePhase:
                case GameLoopPhase.SkillSpawnPhase:
                case GameLoopPhase.VisualStateUpdatePhase:
                    foreach (var (_, skillControllersList) in skillControllersListMap)
                    {
                        foreach (var skillController in skillControllersList)
                        {
                            skillController.OnGameLoopPhase(phase);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        public void OnSpawn()
        {
            foreach (var skillController in skillControllersListMap[PassiveSkillTriggerType.OnSpawn])
            {
                AddActivateSkillAction(skillController);
            }
        }

        public void OnDead(NetworkObject? damageOwner)
        {
            foreach (var skillController in skillControllersListMap[PassiveSkillTriggerType.OnDead])
            {
                AddActivateSkillAction(skillController, damageOwner);
            }
        }

        public void OnTakenDamage(float damageValue, NetworkObject? damageOwner)
        {
            foreach (var skillController in skillControllersListMap[PassiveSkillTriggerType.OnTakenDamage])
            {
                if (skillController.PassiveSkillTrigger is DamagePassiveSkillTrigger damageTrigger
                    && damageValue >= damageTrigger.MinDamageValue)
                {
                    AddActivateSkillAction(skillController, damageOwner);
                }
            }
        }

        public void OnTakenHeal(PlayerRef skillOwner, float healValue, NetworkObject? healOwner)
        {
            foreach (var skillController in skillControllersListMap[PassiveSkillTriggerType.OnTakenHeal])
            {
                if (skillController.PassiveSkillTrigger is HealPassiveSkillTrigger healTrigger
                    && healValue >= healTrigger.MinHealValue)
                {
                    AddActivateSkillAction(skillController, healOwner);
                }
            }
        }

        private void AddActivateSkillAction(SkillController skillController, NetworkObject? selectedTarget = null)
        {
            activateSkillActions.Add(new KeyValuePair<SkillController, NetworkObject?>(skillController, selectedTarget));
        }

        private void ActivateSkill(SkillController skillController,
            NetworkObject? selectedTarget = null)
        {
            switch (skillController.ActivationType)
            {
                case SkillActivationType.WithUnitTarget when selectedTarget != null:
                    skillController.Activate();
                    skillController.ApplyUnitTarget(selectedTarget);
                    skillController.Execute();
                    break;
                case SkillActivationType.WithUnitTarget when selectedTarget == null:
                    Debug.LogError("PassiveSkillController: ActivationType is UnitTarget and SelectedTarget is null");
                    break;
                case SkillActivationType.Instantly:
                    skillController.Activate();
                    break;
                case SkillActivationType.WithMapPointTarget:
                    Debug.LogError("PassiveSkillController: ActivationType MapPointTarget is not supported");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}