using Main.Scripts.Effects;
using Main.Scripts.Skills.ActiveSkills;
using UnityEngine;

namespace Main.Scripts.Player.Config
{
    [CreateAssetMenu(fileName = "HeroConfig", menuName = "Hero/HeroConfig")]
    public class HeroConfig : ScriptableObject
    {
        [SerializeField]
        private string id = null!;
        [SerializeField]
        [Min(1)]
        private uint maxHealth = 1;
        [SerializeField]
        [Min(0f)]
        private float moveSpeed;
        [SerializeField]
        private ActiveSkillType enableAutoAttackFor;
        [SerializeField]
        private ActiveSkillsConfig activeSkillsConfig;
        [SerializeField]
        private EffectsConfig effectsConfig;

        public string Id => id;
        public uint MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public ref ActiveSkillsConfig ActiveSkillsConfig => ref activeSkillsConfig;
        public ActiveSkillType EnableAutoAttackFor => enableAutoAttackFor;
        public ref EffectsConfig EffectsConfig => ref effectsConfig;

        private void OnValidate()
        {
            EffectsManager.OnValidate(name, ref EffectsConfig);
            ActiveSkillsManager.OnValidate(ref ActiveSkillsConfig);
        }
    }
}