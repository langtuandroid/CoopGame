using System;
using System.Collections.Generic;
using Main.Scripts.Effects.Stats;
using Main.Scripts.Effects.Stats.Modifiers;
using UnityEngine;
using WebSocketSharp;

namespace Main.Scripts.Effects
{
    public class EffectsBank : MonoBehaviour
    {
        [SerializeField]
        private List<EffectConfigBase> effects = new();
        [SerializeField]
        private List<EffectsCombination> effectsCombinations = new();

        private Dictionary<int, EffectConfigBase> effectsMap = default!;
        private Dictionary<string, int> effectsIds = default!;
        
        private Dictionary<int, EffectsCombination> effectsCombinationsMap = default!;
        private Dictionary<string, int> effectsCombinationsIds = default!;

        private void Awake()
        {
            effectsMap = new Dictionary<int, EffectConfigBase>(effects.Count);
            effectsIds = new Dictionary<string, int>(effects.Count);
            
            effectsCombinationsMap = new Dictionary<int, EffectsCombination>(effects.Count);
            effectsCombinationsIds = new Dictionary<string, int>(effects.Count);

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

            for (var i = 0; i < effectsCombinations.Count; i++)
            {
                var effectsCombination = effectsCombinations[i];
                if (effectsCombinationsIds.ContainsKey(effectsCombination.NameId))
                {
                    throw new ArgumentException($"EffectsCombination NameId {effectsCombination.NameId} is using by more then one effects combinations");
                }

                effectsCombinationsMap.Add(i, effectsCombination);
                effectsCombinationsIds.Add(effectsCombination.NameId, i);
            }
        }

        private void OnValidate()
        {
            var effectsIdsSet = new HashSet<string>();
            var effectsCombinationsIdsSet = new HashSet<string>();
            effects.Clear();
            effectsCombinations.Clear();

            var effectsObjects = Resources.LoadAll("Scriptable/Effects", typeof(EffectConfigBase));
            foreach (var effectObject in effectsObjects)
            {
                if (effectObject is EffectConfigBase effect)
                {
                    if (effect.NameId.IsNullOrEmpty())
                    {
                        throw new ArgumentException($"{effect.name}: effect NameId is empty");
                    }

                    if (effectsIdsSet.Contains(effect.NameId))
                    {
                        throw new ArgumentException(
                            $"{effect.name}: effect NameId {effect.NameId} is using in more then one effects");
                    }
                    
                    if (effect is StatModifierEffectConfig { StatType: StatType.ReservedDoNotUse })
                    {
                        throw new ArgumentException(
                            $"{effect.name}: unavailable stat type \"ReservedDoNotUse\" in modifier");
                    }

                    effects.Add(effect);
                    effectsIdsSet.Add(effect.NameId);
                }
            }

            var effectsCombinationsObjects = Resources.LoadAll("Scriptable/EffectsCombinations", typeof(EffectsCombination));
            foreach (var effectsCombinationObject in effectsCombinationsObjects)
            {
                if (effectsCombinationObject is EffectsCombination effectsCombination)
                {
                    if (effectsCombination.NameId.IsNullOrEmpty())
                    {
                        throw new ArgumentException($"{effectsCombination.name}: effectsCombination NameId is empty");
                    }

                    if (effectsCombinationsIdsSet.Contains(effectsCombination.NameId))
                    {
                        throw new ArgumentException(
                            $"{effectsCombination.name}: effectsCombination NameId {effectsCombination.NameId} is using in more then one effectsCombinations");
                    }
                    
                    effectsCombinations.Add(effectsCombination);
                    effectsCombinationsIdsSet.Add(effectsCombination.NameId);
                }
            }
        }

        public EffectConfigBase GetEffect(int id)
        {
            if (!effectsMap.ContainsKey(id))
            {
                throw new ArgumentException($"Effect Id {id} is not registered in EffectsBank");
            }

            return effectsMap[id];
        }

        public int GetEffectId(EffectConfigBase effectConfig)
        {
            if (!effectsIds.ContainsKey(effectConfig.NameId))
            {
                throw new ArgumentException($"{effectConfig.name}: effect is not registered in EffectsBank. Check effect file path.");
            }

            return effectsIds[effectConfig.NameId];
        }

        public EffectsCombination GetEffectsCombination(int id)
        {
            if (!effectsCombinationsMap.ContainsKey(id))
            {
                throw new ArgumentException($"EffectsCombination Id {id} is not registered in EffectsBank");
            }

            return effectsCombinationsMap[id];
        }

        public int GetEffectsCombinationId(EffectsCombination effectsCombination)
        {
            if (!effectsCombinationsIds.ContainsKey(effectsCombination.NameId))
            {
                throw new ArgumentException($"{effectsCombination.name}: effectsCombination is not registered in EffectsBank. Check effectsCombination file path.");
            }

            return effectsCombinationsIds[effectsCombination.NameId];
        }
    }
}