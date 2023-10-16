using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.Skills.PassiveSkills;
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
        private uint maxHealth;
        [SerializeField]
        [Min(0f)]
        private float moveSpeed;
        [SerializeField]
        private ActiveSkillsConfig activeSkillsConfig;
        [SerializeField]
        private PassiveSkillsConfig passiveSkillsConfig;

        public string Id => id;
        public uint MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public ref ActiveSkillsConfig ActiveSkillsConfig => ref activeSkillsConfig;
        public ref PassiveSkillsConfig PassiveSkillsConfig => ref passiveSkillsConfig;

        private void OnValidate()
        {
            PassiveSkillsManager.OnValidate(name, ref PassiveSkillsConfig);
            ActiveSkillsManager.OnValidate(ref ActiveSkillsConfig);
        }
    }
}