using System;
using System.Collections.Generic;
using Main.Scripts.Effects;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills
{
    public class PassiveSkillsManager : MonoBehaviour
    {
        [SerializeField]
        private List<EffectsCombination> effectsCombinations = default!;

        private EffectsManager effectsManager = default!;

        public void OnValidate()
        {
            foreach (var effectsCombination in effectsCombinations)
            {
                if (effectsCombination == null) throw new ArgumentNullException($"{gameObject.name}: has null EffectCombination in PassiveSkillManager");
            }
        }

        private void Awake()
        {
            effectsManager = GetComponent<EffectsManager>();
        }

        public void Init()
        {
            foreach (var effectsCombination in effectsCombinations)
            {
                effectsManager.AddEffects(effectsCombination.Effects);
            }
        }
    }
}