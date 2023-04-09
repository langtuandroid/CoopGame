using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Triggers
{
    [CreateAssetMenu(fileName = "HealTrigger", menuName = "Skill/PassiveTrigger/HealTrigger")]
    public class HealPassiveSkillTrigger : PassiveSkillTriggerBase
    {
        [SerializeField]
        [Min(0f)]
        private float minHealValue;

        public float MinHealValue => minHealValue;
    }
}