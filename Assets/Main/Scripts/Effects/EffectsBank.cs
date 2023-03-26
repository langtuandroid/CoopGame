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
        public static EffectsBank? Instance;

        [SerializeField]
        private List<EffectBase> effects = new();

        private Dictionary<int, EffectBase> effectsMap = new();
        private Dictionary<string, int> effectsIds = new();

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;

            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effectsIds.ContainsKey(effect.Id))
                {
                    throw new ArgumentException($"Effect id {effect.Id} is using in more then one effects");
                }

                effectsMap.Add(i, effect);
                effectsIds.Add(effect.Id, i);
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
                    if (effect.Id.IsNullOrEmpty())
                    {
                        throw new ArgumentException($"{effect.name}: effect Id is empty");
                    }

                    if (ids.Contains(effect.Id))
                    {
                        throw new ArgumentException(
                            $"{effect.name}: effect id {effect.Id} is using in more then one effects");
                    }
                    
                    if (effect is StatModifierEffect { StatType: StatType.ReservedDoNotUse })
                    {
                        throw new ArgumentException(
                            $"{effect.name}: unavailable stat type \"ReservedDoNotUse\" in modifier");
                    }

                    effects.Add(effect);
                    ids.Add(effect.Id);
                }
            }
        }

        private void OnDestroy()
        {
            Instance = null;
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
            if (!effectsIds.ContainsKey(effect.Id))
            {
                throw new ArgumentException($"{effect.name}: effect is not registered in EffectsBank. Check effect file path.");
            }

            return effectsIds[effect.Id];
        }
    }
}