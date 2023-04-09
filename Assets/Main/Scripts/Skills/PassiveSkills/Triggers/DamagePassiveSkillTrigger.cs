using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Triggers
{
    [CreateAssetMenu(fileName = "DamageTrigger", menuName = "Skill/PassiveTrigger/DamageTrigger")]
    public class DamagePassiveSkillTrigger : PassiveSkillTriggerBase
    {
        [SerializeField]
        [Min(0f)]
        private float minDamageValue;

        public float MinDamageValue => minDamageValue;
    }
}