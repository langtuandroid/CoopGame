using Cysharp.Threading.Tasks;
using Fusion;
using Main.Scripts.Customization.Banks;
using Main.Scripts.Effects;
using Main.Scripts.Mobs.Config;
using Main.Scripts.Modifiers;
using Main.Scripts.Player.Config;
using Main.Scripts.Skills;
using Main.Scripts.Skills.Common.Component;
using Main.Scripts.UI.Windows.HUD.HotBar;
using UnityEngine;

namespace Main.Scripts.Core.Resources
{
    public class GlobalResources : MonoBehaviour
    {
        public static GlobalResources? Instance { get; private set; }

        [SerializeField]
        private CustomizationConfigsBank customizationConfigsBank = null!;
        [SerializeField]
        private SkillInfoBank skillInfoBank = null!;
        [SerializeField]
        private SkillConfigsBank skillConfigsBank = null!;
        [SerializeField]
        private EffectsBank effectsBank = null!;
        [SerializeField]
        private ModifierIdsBank modifierIdsBank = null!;
        [SerializeField]
        private HeroConfigsBank heroConfigsBank = null!;
        [SerializeField]
        private MobConfigsBank mobConfigsBank = null!;
        [SerializeField]
        private HotBarIconDataHolder hotBarIconDataHolder = null!;

        public CustomizationConfigsBank CustomizationConfigsBank => customizationConfigsBank;
        public SkillInfoBank SkillInfoBank => skillInfoBank;
        public SkillConfigsBank SkillConfigsBank => skillConfigsBank;
        public EffectsBank EffectsBank => effectsBank;
        public ModifierIdsBank ModifierIdsBank => modifierIdsBank;
        public HeroConfigsBank HeroConfigsBank => heroConfigsBank;
        public MobConfigsBank MobConfigsBank => mobConfigsBank;
        public HotBarIconDataHolder HotBarIconDataHolder => hotBarIconDataHolder;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        public async UniTask Init()
        {
            await customizationConfigsBank.Init();
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}