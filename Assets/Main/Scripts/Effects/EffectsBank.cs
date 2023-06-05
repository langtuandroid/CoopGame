using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Effects.Stats.Modifiers;
using UnityEngine;
using WebSocketSharp;

namespace Main.Scripts.Effects
{
    public class EffectsBank : MonoBehaviour
    {
        public const int UNLIMITED_EFFECTS_COUNT = 10;
        public const int LIMITED_EFFECTS_COUNT = 10;

        [SerializeField]
        private List<EffectBase> effects = new();

        private Dictionary<int, EffectBase> effectsMap = default!;
        private Dictionary<string, int> effectsIds = default!;

        private void Awake()
        {
            effectsMap = new Dictionary<int, EffectBase>(effects.Count);
            effectsIds = new Dictionary<string, int>(effects.Count);

            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effectsIds.ContainsKey(effect.NameId))
                {
                    throw new ArgumentException($"Effect NameId {effect.NameId} is using by more then one effect");
                }

                effectsMap.Add(i, effect);
                effectsIds.Add(effect.NameId, i);
            }
        }

        private void OnValidate()
        {
            var ids = new HashSet<string>();
            effects.Clear();

            var effectsObjects = Resources.LoadAll("Scriptable/Effects", typeof(EffectBase));
            foreach (var effectObject in effectsObjects)
            {
                if (effectObject is EffectBase effect)
                {
                    if (effect.NameId.IsNullOrEmpty())
                    {
                        throw new ArgumentException($"{effect.name}: effect NameId is empty");
                    }

                    if (ids.Contains(effect.NameId))
                    {
                        throw new ArgumentException(
                            $"{effect.name}: effect NameId {effect.NameId} is using in more then one effects");
                    }
                    
                    if (effect is StatModifierEffect { StatType: StatType.ReservedDoNotUse })
                    {
                        throw new ArgumentException(
                            $"{effect.name}: unavailable stat type \"ReservedDoNotUse\" in modifier");
                    }

                    effects.Add(effect);
                    ids.Add(effect.NameId);
                }
            }
            
            var (unlimitedCount, limitedCount) = GetUnlimitedAndLimitedEffectsCounts();
            if (unlimitedCount != UNLIMITED_EFFECTS_COUNT)
            {
                Debug.LogWarning($"The UNLIMITED_EFFECTS_COUNT is not equal to the registered value: {unlimitedCount}");
            }
            if (limitedCount != LIMITED_EFFECTS_COUNT)
            {
                Debug.LogWarning($"The LIMITED_EFFECTS_COUNT is not equal to the registered value: {limitedCount}");
            }
        }

        public EffectBase GetEffect(int id)
        {
            if (!effectsMap.ContainsKey(id))
            {
                throw new ArgumentException($"Effect Id {id} is not registered in EffectsBank");
            }

            return effectsMap[id];
        }

        public int GetEffectId(EffectBase effect)
        {
            if (!effectsIds.ContainsKey(effect.NameId))
            {
                throw new ArgumentException($"{effect.name}: effect is not registered in EffectsBank. Check effect file path.");
            }

            return effectsIds[effect.NameId];
        }

        private KeyValuePair<int, int> GetUnlimitedAndLimitedEffectsCounts()
        {
            var unlimitedEffectsSum = 0;
            var limitedEffectsSum = 0;
            foreach (var effect in effects)
            {
                if (effect.DurationSec == 0)
                {
                    unlimitedEffectsSum++;
                }
                else
                {
                    limitedEffectsSum++;
                }
            }

            return new KeyValuePair<int, int>(unlimitedEffectsSum, limitedEffectsSum);
        }
    }
}