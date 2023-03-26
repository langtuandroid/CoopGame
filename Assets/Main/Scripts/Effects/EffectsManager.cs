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
using UnityEngine.Events;

namespace Main.Scripts.Effects
{
    public class EffectsManager : NetworkBehaviour
    {
        private EffectsBank effectsBank = default!;

        [Networked]
        [Capacity(10)]
        private NetworkLinkedList<ActiveEffectData> stackableUnlimitedEffectDataList => default;
        [Networked]
        [Capacity(10)]
        private NetworkDictionary<int, ActiveEffectData> unstackableUnlimitedEffectDataMap => default;

        [Networked]
        [Capacity(10)]
        private NetworkLinkedList<ActiveEffectData> stackableEffectDataList => default;
        [Networked]
        [Capacity(10)]
        private NetworkDictionary<int, ActiveEffectData> unstackableEffectDataMap => default;

        [Networked]
        [Capacity((int)StatType.ReservedDoNotUse)]
        private NetworkArray<float> statConstAdditiveSums => default;
        [Networked]
        [Capacity((int)StatType.ReservedDoNotUse)]
        private NetworkArray<float> statPercentAdditiveSums => default;


        public UnityEvent<StatType> OnUpdatedStatModifiersEvent = default!;

        private Dictionary<PeriodicEffectType, PeriodicEffectsHandler> periodicEffectsHandlers = new();
        private List<ActiveEffectData> aliveEffectDataList = new();
        private List<int> endedEffectIds = new();
        private HashSet<StatType> updatedStatTypes = new();

        private void Awake()
        {
            effectsBank = EffectsBank.Instance.ThrowWhenNull();
            InitEffectsHandlers();
        }

        public void ResetState()
        {
            stackableUnlimitedEffectDataList.Clear();
            unstackableUnlimitedEffectDataMap.Clear();
            stackableEffectDataList.Clear();
            unstackableEffectDataMap.Clear();

            for (var i = 0; i < (int)StatType.ReservedDoNotUse; i++)
            {
                statConstAdditiveSums.Set(i, 0f);
                statPercentAdditiveSums.Set(i, 0f);
            }
        }

        public void UpdateEffects()
        {
            RemoveEndedEffects(Runner.Tick);

            foreach (var data in stackableUnlimitedEffectDataList)
            {
                if (effectsBank.GetEffect(data.EffectId) is PeriodicEffectBase periodicEffect)
                {
                    HandlePeriodicEffect(periodicEffect);
                }
            }

            foreach (var (_, data) in unstackableUnlimitedEffectDataMap)
            {
                if (effectsBank.GetEffect(data.EffectId) is PeriodicEffectBase periodicEffect)
                {
                    HandlePeriodicEffect(periodicEffect);
                }
            }

            foreach (var data in stackableEffectDataList)
            {
                if (effectsBank.GetEffect(data.EffectId) is PeriodicEffectBase periodicEffect)
                {
                    HandlePeriodicEffect(periodicEffect);
                }
            }

            foreach (var (_, data) in unstackableEffectDataMap)
            {
                if (effectsBank.GetEffect(data.EffectId) is PeriodicEffectBase periodicEffect)
                {
                    HandlePeriodicEffect(periodicEffect);
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
            periodicEffectsHandlers.Add(PeriodicEffectType.Heal, new HealPeriodicEffectsHandler());
            periodicEffectsHandlers.Add(PeriodicEffectType.Damage, new DamagePeriodicEffectsHandler());

            foreach (var (_, effectsHandler) in periodicEffectsHandlers)
            {
                effectsHandler.TrySetTarget(gameObject);
            }
        }

        private void HandlePeriodicEffect(PeriodicEffectBase periodicEffect)
        {
            var tick = Runner.Tick;
            var tickRate = Runner.Config.Simulation.TickRate;

            if (tick % (int)(tickRate / periodicEffect.Frequency) != 0) return;


            periodicEffectsHandlers[periodicEffect.PeriodicEffectType].HandleEffect(periodicEffect);
        }

        private void AddEffect(EffectBase effect)
        {
            var endTick = 0;
            if (effect.DurationSec > 0)
            {
                endTick = (int)(Runner.Tick + effect.DurationSec * Runner.Config.Simulation.TickRate);
            }

            var data = new ActiveEffectData(
                effectId: effectsBank.GetEffectId(effect),
                endTick: endTick
            );
            
            var isNewEffect = false;

            if (effect.IsStackable)
            {
                if (data.EndTick > 0)
                {
                    stackableEffectDataList.Add(data);
                }
                else
                {
                    stackableUnlimitedEffectDataList.Add(data);
                }

                isNewEffect = true;
            }
            else
            {
                if (data.EndTick > 0)
                {
                    isNewEffect = !unstackableEffectDataMap.ContainsKey(data.EffectId);
                    unstackableEffectDataMap.Set(data.EffectId, data);
                }
                else
                {
                    isNewEffect = !unstackableUnlimitedEffectDataMap.ContainsKey(data.EffectId);
                    unstackableUnlimitedEffectDataMap.Set(data.EffectId, data);
                }
            }

            if (isNewEffect && effect is StatModifierEffect modifier)
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

            aliveEffectDataList.Clear();
            foreach (var effectData in stackableEffectDataList)
            {
                if (tick <= effectData.EndTick)
                {
                    aliveEffectDataList.Add(effectData);
                }
                else if (effectsBank.GetEffect(effectData.EffectId) is StatModifierEffect modifier)
                {
                    RemoveStatModifier(modifier);
                    updatedStatTypes.Add(modifier.StatType);
                }
            }

            stackableEffectDataList.Clear();
            foreach (var effectData in aliveEffectDataList)
            {
                stackableEffectDataList.Add(effectData);
            }

            endedEffectIds.Clear();
            foreach (var (id, effectData) in unstackableEffectDataMap)
            {
                if (tick > effectData.EndTick)
                {
                    endedEffectIds.Add(id);
                    if (effectsBank.GetEffect(effectData.EffectId) is StatModifierEffect modifier)
                    {
                        RemoveStatModifier(modifier);
                        updatedStatTypes.Add(modifier.StatType);
                    }
                }
            }

            foreach (var id in endedEffectIds)
            {
                unstackableEffectDataMap.Remove(id);
            }

            foreach (var statType in updatedStatTypes)
            {
                OnUpdatedStatModifiersEvent.Invoke(statType);
            }
        }

        private void RemoveStatModifier(StatModifierEffect modifierEffect)
        {
            var statType = modifierEffect.StatType;

            statConstAdditiveSums.Set(
                index: (int)statType,
                value: Math.Max(0, statConstAdditiveSums[(int)statType] - modifierEffect.ConstAdditive)
            );

            statPercentAdditiveSums.Set(
                index: (int)statType,
                value: Math.Max(0, statPercentAdditiveSums[(int)statType] - modifierEffect.PercentAdditive)
            );
        }
    }
}