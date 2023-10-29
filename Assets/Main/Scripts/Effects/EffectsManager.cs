using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Main.Scripts.Core.Architecture;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Effects.Stats.Modifiers;
using Main.Scripts.Effects.TriggerEffects;
using Main.Scripts.Effects.TriggerEffects.Triggers;
using Main.Scripts.Skills.Charge;
using Main.Scripts.Skills.Common.Controller;
using Main.Scripts.Skills.Common.Controller.Interruption;
using UnityEngine;
using UnityEngine.Pool;

namespace Main.Scripts.Effects
{
    public class EffectsManager
    {
        private DataHolder dataHolder;
        private EventListener eventListener;
        private NetworkObject objectContext = null!;
        private bool isPlayerOwner;
        private EffectsConfig config;

        private EffectsBank effectsBank = null!;
        private SkillHeatLevelManager skillHeatLevelManager = null!;

        private Dictionary<EffectType, Dictionary<int, ActiveEffectData>> unlimitedEffectDataMap = new();
        private Dictionary<EffectType, Dictionary<int, ActiveEffectData>> limitedEffectDataMap = new();
        private float[] statConstAdditiveSums = new float[(int)StatType.ReservedDoNotUse];
        private float[] statPercentAdditiveSums = new float[(int)StatType.ReservedDoNotUse];

        private Dictionary<int, EffectSkillController> passiveSkillControllersMap = new();
        private Dictionary<EffectType, List<NetworkId>> effectTargetsIdMap = new();
        private HashSet<EffectType> triggersToActivate = new();
        private List<EffectsCombination> effectAddActions = new();
        private SkillInterruptionType interruptionTypes;

        private HashSet<StatType> updatedStatTypes = new();
        private List<int> removedEffectIds = new();

        public EffectsManager(
            DataHolder dataHolder,
            EventListener eventListener
        )
        {
            this.dataHolder = dataHolder;
            this.eventListener = eventListener;

            foreach (var triggerType in Enum.GetValues(typeof(EffectType)).Cast<EffectType>())
            {
                unlimitedEffectDataMap[triggerType] = new Dictionary<int, ActiveEffectData>();
                limitedEffectDataMap[triggerType] = new Dictionary<int, ActiveEffectData>();
                effectTargetsIdMap[triggerType] = new List<NetworkId>();
            }
        }

        public static void OnValidate(string name, ref EffectsConfig config)
        {
            foreach (var effectsCombination in config.InitialEffects)
            {
                if (effectsCombination == null)
                {
                    throw new ArgumentNullException(
                        $"{name}: has empty value in PassiveSkillsConfig::InitialEffects");
                }

                foreach (var effectConfig in effectsCombination.Effects)
                {
                    if (effectConfig is TriggerEffectConfig triggerEffectConfig)
                    {
                        SkillConfigsValidationHelper.Validate(triggerEffectConfig.SkillControllerConfig);
                    }
                }
            }
        }

        public void Spawned(
            NetworkObject objectContext,
            bool isPlayerOwner,
            ref EffectsConfig config
        )
        {
            this.objectContext = objectContext;
            this.isPlayerOwner = isPlayerOwner;
            this.config = config;
            effectsBank = dataHolder.GetCachedComponent<EffectsBank>();
            skillHeatLevelManager = dataHolder.GetCachedComponent<SkillHeatLevelManager>();
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            ResetState();

            objectContext = null!;
            effectsBank = null!;
            skillHeatLevelManager = null!;
        }

        public void ResetOnRespawn()
        {
            ResetState();
        }

        private void ResetState()
        {
            updatedStatTypes.Clear();
            effectAddActions.Clear();
            interruptionTypes = default;

            foreach (var (_, passiveSkillController) in passiveSkillControllersMap)
            {
                passiveSkillController.Despawned();
                passiveSkillController.Release();
                GenericPool<EffectSkillController>.Release(passiveSkillController);
            }

            passiveSkillControllersMap.Clear();

            foreach (var (_, typedEffectsMap) in unlimitedEffectDataMap)
            {
                typedEffectsMap.Clear();
            }

            foreach (var (_, typedEffectsMap) in limitedEffectDataMap)
            {
                typedEffectsMap.Clear();
            }

            foreach (var (_, targetUnitIdsList) in effectTargetsIdMap)
            {
                targetUnitIdsList.Clear();
            }

            Array.Fill(statConstAdditiveSums, 0f);
            Array.Fill(statPercentAdditiveSums, 0f);
        }


        public void ApplyInitialEffects()
        {
            foreach (var effects in config.InitialEffects)
            {
                ApplyEffects(effects.Effects);
            }
        }

        public float GetModifiedValue(StatType statType, float defaultStatValue)
        {
            var constAdditive = statConstAdditiveSums[(int)statType];

            var percentAdditive = statPercentAdditiveSums[(int)statType];

            return (defaultStatValue + constAdditive) * (percentAdditive + 100) * 0.01f;
        }

        public void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.SkillActivationPhase:
                    ApplyEffectsInterruption();
                    ActivateTriggerEffects();
                    break;
                case GameLoopPhase.SkillCheckSkillFinished:
                case GameLoopPhase.SkillCheckCastFinished:
                case GameLoopPhase.SkillSpawnPhase:
                case GameLoopPhase.VisualStateUpdatePhase:
                    foreach (var (_, skillController) in passiveSkillControllersMap)
                    {
                        skillController.OnGameLoopPhase(phase);
                    }

                    break;
                case GameLoopPhase.EffectsApplyPhase:
                    foreach (var effectAddAction in effectAddActions)
                    {
                        ApplyEffects(effectAddAction.Effects);
                    }

                    effectAddActions.Clear();
                    break;
                case GameLoopPhase.EffectsRemoveFinishedPhase:
                    RemoveFinishedEffects();
                    break;
            }
        }

        private void ActivateTriggerEffects()
        {
            triggersToActivate.Add(EffectType.CooldownTrigger);
            foreach (var triggerType in triggersToActivate)
            {
                var effectTargetsIdList = effectTargetsIdMap[triggerType];
                foreach (var (_, data) in unlimitedEffectDataMap[triggerType])
                {
                    if (effectsBank.GetEffect(data.EffectId) is TriggerEffectConfig triggerEffect)
                    {
                        HandleTriggerEffect(in data, triggerEffect, effectTargetsIdList);
                    }
                }

                foreach (var (_, data) in limitedEffectDataMap[EffectType.CooldownTrigger])
                {
                    if (effectsBank.GetEffect(data.EffectId) is TriggerEffectConfig triggerEffect)
                    {
                        HandleTriggerEffect(in data, triggerEffect, effectTargetsIdList);
                    }
                }
                
                effectTargetsIdMap[triggerType].Clear();
            }

            triggersToActivate.Clear();
        }

        public void AddEffects(EffectsCombination effectsCombination)
        {
            effectAddActions.Add(effectsCombination);
        }

        private void ApplyEffects(IEnumerable<EffectConfigBase> effects)
        {
            updatedStatTypes.Clear();

            foreach (var effect in effects)
            {
                ApplyEffect(effect);
                if (effect is StatModifierEffectConfig modifier)
                {
                    updatedStatTypes.Add(modifier.StatType);
                }
            }

            foreach (var statType in updatedStatTypes)
            {
                eventListener.OnUpdatedStatModifiers(statType);
            }
        }

        private void ApplyEffect(EffectConfigBase effectConfig)
        {
            var startTick = objectContext.Runner.Tick;
            var endTick = 0;
            var effectId = effectsBank.GetEffectId(effectConfig);
            var effectType = GetEffectType(effectConfig);
            ActiveEffectData? currentData = null;
            if (effectConfig.DurationTicks > 0)
            {
                endTick = startTick + effectConfig.DurationTicks;
                if (limitedEffectDataMap.TryGetValue(effectType, out var typedLimitedMap)
                    && typedLimitedMap.ContainsKey(effectId))
                {
                    currentData = typedLimitedMap[effectId];
                }
            }
            else
            {
                if (unlimitedEffectDataMap.TryGetValue(effectType, out var typedUnlimitedMap)
                    && typedUnlimitedMap.ContainsKey(effectId))
                {
                    currentData = typedUnlimitedMap[effectId];
                }
            }


            var newData = new ActiveEffectData(
                effectId: effectId,
                startTick: startTick,
                endTick: endTick,
                stackCount: Math.Min(currentData?.StackCount ?? 0 + 1, effectConfig.MaxStackCount)
            );

            if (effectConfig.IsUnlimited)
            {
                unlimitedEffectDataMap[GetEffectType(effectConfig)][effectId] = newData;
            }
            else
            {
                limitedEffectDataMap[GetEffectType(effectConfig)][effectId] = newData;
            }

            if (currentData?.StackCount != newData.StackCount && effectConfig is StatModifierEffectConfig modifier)
            {
                ApplyNewStatModifier(modifier);
            }
        }

        private void ApplyNewStatModifier(StatModifierEffectConfig modifierEffectConfig)
        {
            var statType = modifierEffectConfig.StatType;

            UpdateStatAdditiveSum(
                statType: statType,
                constValue: statConstAdditiveSums[(int)statType] + modifierEffectConfig.ConstAdditive,
                percentValue: statPercentAdditiveSums[(int)statType] + modifierEffectConfig.PercentAdditive
            );
        }

        private void UpdateStatAdditiveSum(StatType statType, float constValue, float percentValue)
        {
            statConstAdditiveSums[(int)statType] = constValue;
            statPercentAdditiveSums[(int)statType] = percentValue;
        }

        private void HandleTriggerEffect(
            in ActiveEffectData data,
            TriggerEffectConfig triggerEffectConfig,
            List<NetworkId> effectTargetsIdList
        )
        {
            if (!passiveSkillControllersMap.TryGetValue(data.EffectId, out var passiveSkillController))
            {
                passiveSkillController = GenericPool<EffectSkillController>.Get();

                passiveSkillController.Init(
                    passiveSkillTrigger: triggerEffectConfig.Trigger,
                    skillControllerConfig: triggerEffectConfig.SkillControllerConfig,
                    selfUnitTransform: objectContext.transform,
                    alliesLayerMask: config.AlliesLayerMask,
                    opponentsLayerMask: config.OpponentsLayerMask
                );
                passiveSkillController.Spawned(objectContext, isPlayerOwner);
                passiveSkillControllersMap[data.EffectId] = passiveSkillController;
            }

            switch (passiveSkillController.ActivationType)
            {
                case SkillActivationType.WithUnitTarget:
                    Debug.LogError("PassiveSkillController: ActivationType WithUnitTarget is not supported");
                    break;
                case SkillActivationType.Instantly:
                    passiveSkillController.Activate(skillHeatLevelManager.HeatLevel, data.StackCount, effectTargetsIdList);
                    break;
                case SkillActivationType.WithMapPointTarget:
                    Debug.LogError("PassiveSkillController: ActivationType MapPointTarget is not supported");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RemoveFinishedEffects()
        {
            updatedStatTypes.Clear();
            removedEffectIds.Clear();

            foreach (var (_, typedLimitedEffectsMap) in limitedEffectDataMap)
            {
                foreach (var (effectId, effectData) in typedLimitedEffectsMap)
                {
                    if (objectContext.Runner.Tick > effectData.EndTick)
                    {
                        if (effectsBank.GetEffect(effectData.EffectId) is StatModifierEffectConfig modifier)
                        {
                            RemoveStatModifier(modifier, effectData.StackCount);
                            updatedStatTypes.Add(modifier.StatType);
                        }

                        if (passiveSkillControllersMap.TryGetValue(effectId, out var passiveSkillController))
                        {
                            passiveSkillControllersMap.Remove(effectId);
                            passiveSkillController.Despawned();
                            passiveSkillController.Release();
                            GenericPool<EffectSkillController>.Release(passiveSkillController);
                        }

                        removedEffectIds.Add(effectId);
                    }
                }

                foreach (var effectId in removedEffectIds)
                {
                    typedLimitedEffectsMap.Remove(effectId);
                }
            }

            foreach (var statType in updatedStatTypes)
            {
                eventListener.OnUpdatedStatModifiers(statType);
            }
        }

        private void RemoveStatModifier(StatModifierEffectConfig modifierEffectConfig, int stackCount)
        {
            var statType = modifierEffectConfig.StatType;

            UpdateStatAdditiveSum(
                statType: statType,
                constValue: Math.Max(0,
                    statConstAdditiveSums[(int)statType] - modifierEffectConfig.ConstAdditive * stackCount),
                percentValue: Math.Max(0,
                    statPercentAdditiveSums[(int)statType] - modifierEffectConfig.PercentAdditive * stackCount)
            );
        }

        public void AddEffectsInterruption(SkillInterruptionType interruptionType)
        {
            interruptionTypes |= interruptionType;
        }

        private void ApplyEffectsInterruption()
        {
            if (interruptionTypes == 0) return;

            foreach (var (_, skillController) in passiveSkillControllersMap)
            {
                skillController.TryInterrupt(interruptionTypes);
            }
        }

        public void OnSpawn()
        {
            triggersToActivate.Add(EffectType.SpawnTrigger);
        }

        public void OnDead()
        {
            triggersToActivate.Add(EffectType.DeadTrigger);
        }

        public void OnTakenDamage(float damageValue, NetworkObject? damageOwner)
        {
            triggersToActivate.Add(EffectType.TakenDamageTrigger);
            if (damageOwner != null && damageOwner != objectContext)
            {
                effectTargetsIdMap[EffectType.DeadTrigger].Add(damageOwner.Id);
                effectTargetsIdMap[EffectType.TakenDamageTrigger].Add(damageOwner.Id);
            }
        }

        public void OnTakenHeal(PlayerRef skillOwner, float healValue, NetworkObject? healOwner)
        {
            triggersToActivate.Add(EffectType.TakenHealTrigger);
            if (healOwner != null && healOwner != objectContext)
            {
                effectTargetsIdMap[EffectType.TakenHealTrigger].Add(healOwner.Id);
            }
        }

        private EffectType GetEffectType(EffectConfigBase effectConfig)
        {
            if (effectConfig is StatModifierEffectConfig)
            {
                return EffectType.StatModifier;
            }

            if (effectConfig is TriggerEffectConfig triggerEffectConfig)
            {
                return triggerEffectConfig.Trigger switch
                {
                    OnCooldownEffectTrigger => EffectType.CooldownTrigger,
                    SpawnEffectTrigger => EffectType.SpawnTrigger,
                    DeadEffectTrigger => EffectType.DeadTrigger,
                    TakenDamageEffectTrigger => EffectType.TakenDamageTrigger,
                    TakenHealEffectTrigger => EffectType.TakenHealTrigger,
                    _ => throw new ArgumentOutOfRangeException(nameof(triggerEffectConfig.Trigger),
                        triggerEffectConfig.Trigger, null)
                };
            }

            throw new ArgumentException($"Effect type {effectConfig.GetType()} is not implemented");
        }

        public interface DataHolder : ComponentsHolder { }

        public interface EventListener
        {
            public void OnUpdatedStatModifiers(StatType statType);
        }
    }
}