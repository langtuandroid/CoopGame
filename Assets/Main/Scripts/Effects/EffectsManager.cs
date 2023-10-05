using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.Architecture;
using Main.Scripts.Effects.PeriodicEffects;
using Main.Scripts.Effects.PeriodicEffects.Handlers;
using Main.Scripts.Effects.PeriodicEffects.Handlers.Damage;
using Main.Scripts.Effects.PeriodicEffects.Handlers.Heal;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Effects.Stats.Modifiers;

namespace Main.Scripts.Effects
{
    public class EffectsManager: EffectDataChangeListener
    {
        private DataHolder dataHolder;
        private EventListener eventListener;
        private NetworkObject objectContext = default!;

        private EffectsBank effectsBank = default!;
        
        private Dictionary<int, ActiveEffectData> unlimitedEffectDataMap = new();
        private Dictionary<int, ActiveEffectData> limitedEffectDataMap = new();
        private float[] statConstAdditiveSums = new float[(int)StatType.ReservedDoNotUse];
        private float[] statPercentAdditiveSums = new float[(int)StatType.ReservedDoNotUse];

        private Dictionary<PeriodicEffectType, PeriodicEffectsHandler> periodicEffectsHandlers = new();
        private List<List<ActiveEffectData>> periodicEffectsDataToHandle = new();
        private List<int> endedEffectIds = new();
        private HashSet<StatType> updatedStatTypes = new();

        public EffectsManager(
            DataHolder dataHolder,
            EventListener eventListener,
            object effectsTarget
        )
        {
            this.dataHolder = dataHolder;
            this.eventListener = eventListener;

            InitEffectsHandlers(effectsTarget);
        }

        public void Spawned(NetworkObject objectContext)
        {
            this.objectContext = objectContext;
            effectsBank = dataHolder.GetCachedComponent<EffectsBank>();
            dataHolder.SetEffectDataChangeListener(this);
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            ResetState();

            objectContext = default!;
            effectsBank = default!;

            dataHolder.SetEffectDataChangeListener(null);
        }

        public void ResetOnRespawn()
        {
            dataHolder.ResetAllEffectData();
        }

        private void ResetState()
        {
            endedEffectIds.Clear();
            updatedStatTypes.Clear();
                
            unlimitedEffectDataMap.Clear();
            limitedEffectDataMap.Clear();
            Array.Fill(statConstAdditiveSums, 0f);
            Array.Fill(statPercentAdditiveSums, 0f);
        }

        public void UpdateEffects()
        {
            foreach (var periodicEffectsList in periodicEffectsDataToHandle)
            {
                periodicEffectsList.Clear();
            }

            foreach (var (_, data) in unlimitedEffectDataMap)
            {
                if (effectsBank.GetEffect(data.EffectId) is PeriodicEffectBase periodicEffect)
                {
                    periodicEffectsDataToHandle[(int)periodicEffect.PeriodicEffectType].Add(data);
                }
            }

            foreach (var (_, data) in limitedEffectDataMap)
            {
                if (effectsBank.GetEffect(data.EffectId) is PeriodicEffectBase periodicEffect)
                {
                    periodicEffectsDataToHandle[(int)periodicEffect.PeriodicEffectType].Add(data);
                }
            }

            foreach (var dataList in periodicEffectsDataToHandle)
            {
                foreach (var data in dataList)
                {
                    if (effectsBank.GetEffect(data.EffectId) is PeriodicEffectBase periodicEffect)
                    {
                        HandlePeriodicEffect(periodicEffect, data.StartTick, data.StackCount);
                    }
                }
            }
        }

        public void AddEffects(IEnumerable<EffectBase> effects)
        {
            updatedStatTypes.Clear();
            foreach (var effect in effects)
            {
                AddEffect(effect);
                if (effect is StatModifierEffect modifier)
                {
                    updatedStatTypes.Add(modifier.StatType);
                }
            }

            foreach (var statType in updatedStatTypes)
            {
                eventListener.OnUpdatedStatModifiers(statType);
            }
        }

        public float GetModifiedValue(StatType statType, float defaultStatValue)
        {
            var constAdditive = statConstAdditiveSums[(int)statType];

            var percentAdditive = statPercentAdditiveSums[(int)statType];

            return (defaultStatValue + constAdditive) * (percentAdditive + 100) * 0.01f;
        }
        
        public void OnUpdateEffectData(int effectId, ref ActiveEffectData activeEffectData, bool isUnlimitedEffect)
        {
            if (isUnlimitedEffect)
            {
                unlimitedEffectDataMap[effectId] = activeEffectData;
            }
            else
            {
                limitedEffectDataMap[effectId] = activeEffectData;
            }
        }

        public void OnRemoveLimitedEffectData(int effectId)
        {
            limitedEffectDataMap.Remove(effectId);
        }

        public void OnUpdateStatAdditiveSum(StatType statType, float constValue, float percentValue)
        {
            statConstAdditiveSums[(int)statType] = constValue;
            statPercentAdditiveSums[(int)statType] = percentValue;
        }

        public void OnResetAllEffectData()
        {
            ResetState();
        }

        private void InitEffectsHandlers(object effectsTarget)
        {
            periodicEffectsHandlers.Add(PeriodicEffectType.Damage, new DamagePeriodicEffectsHandler());
            periodicEffectsHandlers.Add(PeriodicEffectType.Heal, new HealPeriodicEffectsHandler());

            foreach (var (_, effectsHandler) in periodicEffectsHandlers)
            {
                effectsHandler.TrySetTarget(effectsTarget);
            }

            var typesCount = Enum.GetValues(typeof(PeriodicEffectType)).Length;
            for (var i = 0; i < typesCount; i++)
            {
                periodicEffectsDataToHandle.Add(new List<ActiveEffectData>());
            }
        }

        private void HandlePeriodicEffect(PeriodicEffectBase periodicEffect, int startTick, int stackCount)
        {
            var tick = objectContext.Runner.Tick;

            if ((tick - startTick) % periodicEffect.FrequencyTicks == 0)
            {
                periodicEffectsHandlers[periodicEffect.PeriodicEffectType].HandleEffect(periodicEffect, stackCount);
            }
        }

        private void AddEffect(EffectBase effect)
        {
            var startTick = objectContext.Runner.Tick;
            var endTick = 0;
            var effectId = effectsBank.GetEffectId(effect);
            ActiveEffectData? currentData = null;
            if (effect.DurationTicks > 0)
            {
                endTick = startTick + effect.DurationTicks;
                if (limitedEffectDataMap.ContainsKey(effectId))
                {
                    currentData = limitedEffectDataMap[effectId];
                }
            }
            else
            {
                if (unlimitedEffectDataMap.ContainsKey(effectId))
                {
                    currentData = unlimitedEffectDataMap[effectId];
                }
            }


            var newData = new ActiveEffectData(
                effectId: effectId,
                startTick: startTick,
                endTick: endTick,
                stackCount: Math.Min(currentData?.StackCount ?? 0 + 1, effect.MaxStackCount)
            );

            if (newData.EndTick > 0)
            {
                dataHolder.UpdateEffectData(newData.EffectId, ref newData, false);
            }
            else
            {
                dataHolder.UpdateEffectData(newData.EffectId, ref newData, true);
            }

            if (currentData?.StackCount != newData.StackCount && effect is StatModifierEffect modifier)
            {
                ApplyNewStatModifier(modifier);
            }
        }

        private void ApplyNewStatModifier(StatModifierEffect modifierEffect)
        {
            var statType = modifierEffect.StatType;

            dataHolder.UpdateStatAdditiveSum(
                statType: statType,
                constValue: statConstAdditiveSums[(int)statType] + modifierEffect.ConstAdditive,
                percentValue: statPercentAdditiveSums[(int)statType] + modifierEffect.PercentAdditive
            );
        }

        public void RemoveFinishedEffects()
        {
            endedEffectIds.Clear();
            updatedStatTypes.Clear();

            foreach (var (id, effectData) in limitedEffectDataMap)
            {
                if (objectContext.Runner.Tick > effectData.EndTick)
                {
                    endedEffectIds.Add(id);
                    if (effectsBank.GetEffect(effectData.EffectId) is StatModifierEffect modifier)
                    {
                        RemoveStatModifier(modifier, effectData.StackCount);
                        updatedStatTypes.Add(modifier.StatType);
                    }
                }
            }

            foreach (var id in endedEffectIds)
            {
                dataHolder.RemoveLimitedEffectData(id);
            }

            foreach (var statType in updatedStatTypes)
            {
                eventListener.OnUpdatedStatModifiers(statType);
            }
        }

        private void RemoveStatModifier(StatModifierEffect modifierEffect, int stackCount)
        {
            var statType = modifierEffect.StatType;
            
            dataHolder.UpdateStatAdditiveSum(
                statType: statType,
                constValue: Math.Max(0, statConstAdditiveSums[(int)statType] - modifierEffect.ConstAdditive * stackCount),
                percentValue: Math.Max(0, statPercentAdditiveSums[(int)statType] - modifierEffect.PercentAdditive * stackCount)
            );
        }

        public interface DataHolder : ComponentsHolder, EffectDataChanger {}

        public interface EventListener
        {
            public void OnUpdatedStatModifiers(StatType statType);
        }
    }
}