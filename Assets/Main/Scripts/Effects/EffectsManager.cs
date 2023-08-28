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
using Main.Scripts.Utils;

namespace Main.Scripts.Effects
{
    public class EffectsManager
    {
        private DataHolder dataHolder;
        private EventListener eventListener;
        private NetworkObject objectContext = default!;

        private EffectsBank effectsBank = default!;

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
        }

        public void Despawned(NetworkRunner runner, bool hasState)
        {
            objectContext = default!;
            effectsBank = default!;
        }

        public void ResetState()
        {
            ref var effectsData = ref dataHolder.GetEffectsData();

            effectsData.unlimitedEffectDataMap.Clear();
            effectsData.limitedEffectDataMap.Clear();

            for (var i = 0; i < (int)StatType.ReservedDoNotUse; i++)
            {
                effectsData.statConstAdditiveSums.Set(i, 0f);
                effectsData.statPercentAdditiveSums.Set(i, 0f);
            }
        }

        public void UpdateEffects()
        {
            ref var effectsData = ref dataHolder.GetEffectsData();

            foreach (var periodicEffectsList in periodicEffectsDataToHandle)
            {
                periodicEffectsList.Clear();
            }

            foreach (var (_, data) in effectsData.unlimitedEffectDataMap)
            {
                if (effectsBank.GetEffect(data.EffectId) is PeriodicEffectBase periodicEffect)
                {
                    periodicEffectsDataToHandle[(int)periodicEffect.PeriodicEffectType].Add(data);
                }
            }

            foreach (var (_, data) in effectsData.limitedEffectDataMap)
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
            ref var effectsData = ref dataHolder.GetEffectsData();

            updatedStatTypes.Clear();
            foreach (var effect in effects)
            {
                AddEffect(ref effectsData, effect);
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
            ref var effectsData = ref dataHolder.GetEffectsData();

            var constAdditive = effectsData.statConstAdditiveSums[(int)statType];

            var percentAdditive = effectsData.statPercentAdditiveSums[(int)statType];

            return (defaultStatValue + constAdditive) * (percentAdditive + 100) * 0.01f;
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
            var tickRate = objectContext.Runner.Config.Simulation.TickRate;

            if (TickHelper.CheckFrequency(tick - startTick, tickRate, periodicEffect.Frequency))
            {
                periodicEffectsHandlers[periodicEffect.PeriodicEffectType].HandleEffect(periodicEffect, stackCount);
            }
        }

        private void AddEffect(ref EffectsData effectsData, EffectBase effect)
        {
            var startTick = objectContext.Runner.Tick;
            var endTick = 0;
            var effectId = effectsBank.GetEffectId(effect);
            ActiveEffectData? currentData = null;
            if (effect.DurationSec > 0)
            {
                endTick = (int)(startTick +
                                effect.DurationSec * objectContext.Runner.Config.Simulation.TickRate);
                if (effectsData.limitedEffectDataMap.ContainsKey(effectId))
                {
                    currentData = effectsData.limitedEffectDataMap.Get(effectId);
                }
            }
            else
            {
                if (effectsData.unlimitedEffectDataMap.ContainsKey(effectId))
                {
                    currentData = effectsData.unlimitedEffectDataMap.Get(effectId);
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
                effectsData.limitedEffectDataMap.Set(newData.EffectId, newData);
            }
            else
            {
                effectsData.unlimitedEffectDataMap.Set(newData.EffectId, newData);
            }

            if (currentData?.StackCount != newData.StackCount && effect is StatModifierEffect modifier)
            {
                ApplyNewStatModifier(ref effectsData, modifier);
            }
        }

        private void ApplyNewStatModifier(ref EffectsData effectsData, StatModifierEffect modifierEffect)
        {
            var statType = modifierEffect.StatType;

            effectsData.statConstAdditiveSums.Set((int)statType,
                effectsData.statConstAdditiveSums[(int)statType] + modifierEffect.ConstAdditive);

            effectsData.statPercentAdditiveSums.Set((int)statType,
                effectsData.statPercentAdditiveSums[(int)statType] + modifierEffect.PercentAdditive);
        }

        public void RemoveFinishedEffects()
        {
            ref var effectsData = ref dataHolder.GetEffectsData();
            updatedStatTypes.Clear();

            endedEffectIds.Clear();
            foreach (var (id, effectData) in effectsData.limitedEffectDataMap)
            {
                if (objectContext.Runner.Tick > effectData.EndTick)
                {
                    endedEffectIds.Add(id);
                    if (effectsBank.GetEffect(effectData.EffectId) is StatModifierEffect modifier)
                    {
                        RemoveStatModifier(ref effectsData, modifier, effectData.StackCount);
                        updatedStatTypes.Add(modifier.StatType);
                    }
                }
            }

            foreach (var id in endedEffectIds)
            {
                effectsData.limitedEffectDataMap.Remove(id);
            }

            foreach (var statType in updatedStatTypes)
            {
                eventListener.OnUpdatedStatModifiers(statType);
            }
        }

        private void RemoveStatModifier(ref EffectsData effectsData, StatModifierEffect modifierEffect, int stackCount)
        {
            var statType = modifierEffect.StatType;

            effectsData.statConstAdditiveSums.Set(
                index: (int)statType,
                value: Math.Max(0,
                    effectsData.statConstAdditiveSums[(int)statType] - modifierEffect.ConstAdditive * stackCount)
            );

            effectsData.statPercentAdditiveSums.Set(
                index: (int)statType,
                value: Math.Max(0,
                    effectsData.statPercentAdditiveSums[(int)statType] - modifierEffect.PercentAdditive * stackCount)
            );
        }

        public interface DataHolder : ComponentsHolder
        {
            public ref EffectsData GetEffectsData();
        }

        public interface EventListener
        {
            public void OnUpdatedStatModifiers(StatType statType);
        }
    }
}