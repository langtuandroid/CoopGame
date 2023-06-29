using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Effects;
using Main.Scripts.Skills.Common.Controller;
using Main.Scripts.Skills.PassiveSkills.Triggers;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills
{
    [SimulationBehaviour(
        Stages = (SimulationStages) 8,
        Modes  = (SimulationModes) 8
    )]
    public class PassiveSkillsManager : MonoBehaviour
    {
        [SerializeField]
        private LayerMask alliesLayerMask;
        [SerializeField]
        private LayerMask opponentsLayerMask;

        [SerializeField]
        private List<PassiveSkillController> passiveSkillControllers = new();
        [SerializeField]
        private List<EffectsCombination> initialEffects = default!;

        private EffectsManager effectsManager = default!;
        private Dictionary<PassiveSkillTriggerType, List<PassiveSkillController>> skillControllersMap = new();

        public void OnValidate()
        {
            if (initialEffects.Any(effectsCombination => effectsCombination == null))
            {
                throw new ArgumentNullException(
                    $"{gameObject.name}: has empty value in PassiveSkillManager::InitialEffects");
            }

            if (passiveSkillControllers.Any(skillController => skillController == null))
            {
                throw new ArgumentNullException(
                    $"{gameObject.name}: has empty value in PassiveSkillManager::PassiveSkillControllers");
            }
        }

        private void Awake()
        {
            effectsManager = GetComponent<EffectsManager>();

            foreach (var type in Enum.GetValues(typeof(PassiveSkillTriggerType)).Cast<PassiveSkillTriggerType>())
            {
                skillControllersMap.Add(type, new List<PassiveSkillController>());
            }

            foreach (var skillController in passiveSkillControllers)
            {
                skillController.Init(
                    transform,
                    alliesLayerMask,
                    opponentsLayerMask
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

        public void Init()
        {
            foreach (var effectsCombination in initialEffects)
            {
                effectsManager.AddEffects(effectsCombination.Effects);
            }
        }

        public void SetOwner(PlayerRef owner)
        {
            foreach (var skillController in passiveSkillControllers)
            {
                skillController.SetOwner(owner);
            }
        }

        public void OnSpawn(PlayerRef skillOwner)
        {
            foreach (var skillController in skillControllersMap[PassiveSkillTriggerType.OnSpawn])
            {
                ActivateSkill(skillController, skillOwner);
            }
        }

        public void OnDead(PlayerRef skillOwner, NetworkObject? damageOwner)
        {
            foreach (var skillController in skillControllersMap[PassiveSkillTriggerType.OnDead])
            {
                ActivateSkill(skillController, skillOwner, damageOwner);
            }
        }

        public void OnTakenDamage(PlayerRef skillOwner, float damageValue, NetworkObject? damageOwner)
        {
            foreach (var skillController in skillControllersMap[PassiveSkillTriggerType.OnTakenDamage])
            {
                if (skillController.PassiveSkillTrigger is DamagePassiveSkillTrigger damageTrigger
                    && damageValue >= damageTrigger.MinDamageValue)
                {
                    ActivateSkill(skillController, skillOwner, damageOwner);
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
                    ActivateSkill(skillController, skillOwner, healOwner);
                }
            }
        }

        private void ActivateSkill(SkillController skillController, PlayerRef skillOwner,
            NetworkObject? selectedTarget = null)
        {
            switch (skillController.ActivationType)
            {
                case SkillActivationType.WithUnitTarget when selectedTarget != null:
                    skillController.Activate(skillOwner);
                    skillController.ApplyUnitTarget(selectedTarget);
                    skillController.Execute();
                    break;
                case SkillActivationType.WithUnitTarget when selectedTarget == null:
                    Debug.LogError("PassiveSkillController: ActivationType is UnitTarget and SelectedTarget is null");
                    break;
                case SkillActivationType.Instantly:
                    skillController.Activate(skillOwner);
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