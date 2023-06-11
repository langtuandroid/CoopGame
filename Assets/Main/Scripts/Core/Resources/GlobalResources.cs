using Fusion;
using Main.Scripts.Customization.Banks;
using Main.Scripts.Effects;
using Main.Scripts.Modifiers;
using Main.Scripts.Skills;
using Main.Scripts.Skills.Common.Component;
using UnityEngine;

namespace Main.Scripts.Core.Resources
{
    public class GlobalResources : MonoBehaviour
    {
        public static GlobalResources? Instance { get; private set; }

        [SerializeField]
        private CustomizationConfigsBank customizationConfigsBank = default!;
        [SerializeField]
        private SkillInfoHolder skillInfoHolder = default!;
        [SerializeField]
        private SkillConfigsBank skillConfigsBank = default!;
        [SerializeField]
        private EffectsBank effectsBank = default!;
        [SerializeField]
        private ModifierIdsBank modifierIdsBank = default!;

        public CustomizationConfigsBank CustomizationConfigsBank => customizationConfigsBank;
        public SkillInfoHolder SkillInfoHolder => skillInfoHolder;
        public SkillConfigsBank SkillConfigsBank => skillConfigsBank;
        public EffectsBank EffectsBank => effectsBank;
        public ModifierIdsBank ModifierIdsBank => modifierIdsBank;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}