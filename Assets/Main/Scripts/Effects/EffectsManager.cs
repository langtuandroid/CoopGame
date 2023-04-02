using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Effects.PeriodicEffects;
using Main.Scripts.Effects.PeriodicEffects.Handlers;
using Main.Scripts.Effects.PeriodicEffects.Handlers.Damage;
using Main.Scripts.Effects.PeriodicEffects.Handlers.Heal;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Effects.Stats.Modifiers;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Effects
{
    public class EffectsManager : NetworkBehaviour
    {
        private const int UNLIMITED_EFFECTS_COUNT = 10;
        private const int LIMITED_EFFECTS_COUNT = 10;
        
        private EffectsBank effectsBank = default!;

        [Networked]
        [Capacity(UNLIMITED_EFFECTS_COUNT)]
        private NetworkDictionary<int, ActiveEffectData> unlimitedEffectDataMap => default;

        [Networked]
        [Capacity(LIMITED_EFFECTS_COUNT)]
        private NetworkDictionary<int, ActiveEffectData> limitedEffectDataMap => default;

        [Networked]
        [Capacity((int)StatType.ReservedDoNotUse)]
        private NetworkArray<float> statConstAdditiveSums => default;
        [Networked]
        [Capacity((int)StatType.ReservedDoNotUse)]
        private NetworkArray<float> statPercentAdditiveSums => default;


        public UnityEvent<StatType> OnUpdatedStatModifiersEvent = default!;

        private Dictionary<PeriodicEffectType, PeriodicEffectsHandler> periodicEffectsHandlers = new();
        private List<List<ActiveEffectData>> periodicEffectsDataToHandle = new();
        private List<int> endedEffectIds = new();
        private HashSet<StatType> updatedStatTypes = new();

        private void Awake()
        {
            effectsBank = EffectsBank.Instance.ThrowWhenNull();
            
            var (unlimitedCount, limitedCount) = effectsBank.GetUnlimitedAndLimitedEffectsCounts();
            if (unlimitedCount != UNLIMITED_EFFECTS_COUNT)
            {
                Debug.LogWarning($"The UNLIMITED_EFFECTS_COUNT is not equal to the registered value: {unlimitedCount}");
            }
            if (limitedCount != LIMITED_EFFECTS_COUNT)
            {
                Debug.LogWarning($"The LIMITED_EFFECTS_COUNT is not equal to the registered value: {limitedCount}");
            }
            
            InitEffectsHandlers();
        }

        public void ResetState()
        {
            unlimitedEffectDataMap.Clear();
            limitedEffectDataMap.Clear();

            for (var i = 0; i < (int)StatType.ReservedDoNotUse; i++)
            {
                statConstAdditiveSums.Set(i, 0f);
                statPercentAdditiveSums.Set(i, 0f);
            }
        }

        public void UpdateEffects()
        {
            RemoveEndedEffects(Runner.Tick);

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
                        HandlePeriodicEffect(periodicEffect, data.StackCount);
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
                OnUpdatedStatModifiersEvent.Invoke(statType);
            }
        }

        public float GetModifiedValue(StatType statType, float defaultStatValue)
        {
            var constAdditive = statConstAdditiveSums[(int)statType];

            var percentAdditive = statPercentAdditiveSums[(int)statType];

            return (defaultStatValue + constAdditive) * (percentAdditive + 100) * 0.01f;
        }

        private void InitEffectsHandlers()
        {
            periodicEffectsHandlers.Add(PeriodicEffectType.Damage, new DamagePeriodicEffectsHandler());
            periodicEffectsHandlers.Add(PeriodicEffectType.Heal, new HealPeriodicEffectsHandler());

            foreach (var (_, effectsHandler) in periodicEffectsHandlers)
            {
                effectsHandler.TrySetTarget(gameObject);
            }

            var typesCount = Enum.GetValues(typeof(PeriodicEffectType)).Length;
            for (var i = 0; i < typesCount; i++)
            {
                periodicEffectsDataToHandle.Add(new List<ActiveEffectData>());
            }
        }

        private void HandlePeriodicEffect(PeriodicEffectBase periodicEffect, int stackCount)
        {
            var tick = Runner.Tick;
            var tickRate = Runner.Config.Simulation.TickRate;

            if (tick % (int)(tickRate / periodicEffect.Frequency) != 0) return; //todo учесть выремя старта эффекта


            periodicEffectsHandlers[periodicEffect.PeriodicEffectType].HandleEffect(periodicEffect, stackCount);
        }

        private void AddEffect(EffectBase effect)
        {
            var endTick = 0;
            var effectId = effectsBank.GetEffectId(effect);
            ActiveEffectData? currentData = null;
            if (effect.DurationSec > 0)
            {
                endTick = (int)(Runner.Tick + effect.DurationSec * Runner.Config.Simulation.TickRate);
                if (limitedEffectDataMap.ContainsKey(effectId))
                {
                    currentData = limitedEffectDataMap.Get(effectId);
                }
            }
            else
            {
                if (unlimitedEffectDataMap.ContainsKey(effectId))
                {
                    currentData = unlimitedEffectDataMap.Get(effectId);
                }
            }


            var newData = new ActiveEffectData(
                effectId: effectId,
                endTick: endTick,
                stackCount: Math.Min(currentData?.StackCount ?? 0 + 1, effect.MaxStackCount)
            );

            if (newData.EndTick > 0)
            {
                limitedEffectDataMap.Set(newData.EffectId, newData);
            }
            else
            {
                unlimitedEffectDataMap.Set(newData.EffectId, newData);
            }

            if (currentData?.StackCount != newData.StackCount && effect is StatModifierEffect modifier)
            {
                ApplyNewStatModifier(modifier);
            }
        }

        private void ApplyNewStatModifier(StatModifierEffect modifierEffect)
        {
            var statType = modifierEffect.StatType;

            statConstAdditiveSums.Set((int)statType,
                statConstAdditiveSums[(int)statType] + modifierEffect.ConstAdditive);

            statPercentAdditiveSums.Set((int)statType,
                statPercentAdditiveSums[(int)statType] + modifierEffect.PercentAdditive);
        }

        private void RemoveEndedEffects(int tick)
        {
            updatedStatTypes.Clear();

            endedEffectIds.Clear();
            foreach (var (id, effectData) in limitedEffectDataMap)
            {
                if (tick > effectData.EndTick)
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
                limitedEffectDataMap.Remove(id);
            }

            foreach (var statType in updatedStatTypes)
            {
                OnUpdatedStatModifiersEvent.Invoke(statType);
            }
        }

        private void RemoveStatModifier(StatModifierEffect modifierEffect, int stackCount)
        {
            var statType = modifierEffect.StatType;

            statConstAdditiveSums.Set(
                index: (int)statType,
                value: Math.Max(0, statConstAdditiveSums[(int)statType] - modifierEffect.ConstAdditive * stackCount)
            );

            statPercentAdditiveSums.Set(
                index: (int)statType,
                value: Math.Max(0, statPercentAdditiveSums[(int)statType] - modifierEffect.PercentAdditive * stackCount)
            );
        }
    }
}