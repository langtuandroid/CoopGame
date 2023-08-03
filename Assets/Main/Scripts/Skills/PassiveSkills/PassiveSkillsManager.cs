using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Skills.Common.Controller;
using Main.Scripts.Skills.PassiveSkills.Triggers;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills
{
    public class PassiveSkillsManager
    {
        private PassiveSkillsConfig config;
        private Affectable affectable;

        private Dictionary<PassiveSkillTriggerType, List<PassiveSkillController>> skillControllersMap = new();

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
                skillControllersMap.Add(type, new List<PassiveSkillController>());
            }

            foreach (var skillController in config.PassiveSkillControllers)
            {
                skillController.Init(
                    transform,
                    config.AlliesLayerMask,
                    config.OpponentsLayerMask
                );

                var type = skillController.PassiveSkillTrigger switch
                {
                    SpawnPassiveSkillTrigger => PassiveSkillTriggerType.OnSpawn,
                    DeadPassiveSkillTrigger => PassiveSkillTriggerType.OnDead,
                    DamagePassiveSkillTrigger => PassiveSkillTriggerType.OnTakenDamage,
                    HealPassiveSkillTrigger => PassiveSkillTriggerType.OnTakenHeal,
                    _ => throw new ArgumentOutOfRangeException(nameof(skillController.PassiveSkillTrigger),
                        skillController.PassiveSkillTrigger, null)
                };

                skillControllersMap[type].Add(skillController);
            }
        }

        public static void OnValidate(GameObject gameObject, ref PassiveSkillsConfig config)
        {
            if (config.InitialEffects.Any(effectsCombination => effectsCombination == null))
            {
                throw new ArgumentNullException(
                    $"{gameObject.name}: has empty value in PassiveSkillsConfig::InitialEffects");
            }

            if (config.PassiveSkillControllers.Any(skillController => skillController == null))
            {
                throw new ArgumentNullException(
                    $"{gameObject.name}: has empty value in PassiveSkillsConfig::PassiveSkillControllers");
            }
        }

        public void Init()
        {
            foreach (var effectsCombination in config.InitialEffects)
            {
                affectable.AddEffects(effectsCombination);
            }
        }

        public void OnSpawn()
        {
            foreach (var skillController in skillControllersMap[PassiveSkillTriggerType.OnSpawn])
            {
                ActivateSkill(skillController);
            }
        }

        public void OnDead(NetworkObject? damageOwner)
        {
            foreach (var skillController in skillControllersMap[PassiveSkillTriggerType.OnDead])
            {
                ActivateSkill(skillController, damageOwner);
            }
        }

        public void OnTakenDamage(float damageValue, NetworkObject? damageOwner)
        {
            foreach (var skillController in skillControllersMap[PassiveSkillTriggerType.OnTakenDamage])
            {
                if (skillController.PassiveSkillTrigger is DamagePassiveSkillTrigger damageTrigger
                    && damageValue >= damageTrigger.MinDamageValue)
                {
                    ActivateSkill(skillController, damageOwner);
                }
            }
        }

        public void OnTakenHeal(PlayerRef skillOwner, float healValue, NetworkObject? healOwner)
        {
            foreach (var skillController in skillControllersMap[PassiveSkillTriggerType.OnTakenHeal])
            {
                if (skillController.PassiveSkillTrigger is HealPassiveSkillTrigger healTrigger
                    && healValue >= healTrigger.MinHealValue)
                {
                    ActivateSkill(skillController, healOwner);
                }
            }
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
                    Debug.LogWarning("PassiveSkillController: ActivationType MapPointTarget is not supported");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}