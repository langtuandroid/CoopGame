using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "HealAction", menuName = "Skill/Action/Heal")]
    public class HealSkillAction : SkillActionBase
    {
        [SerializeField]
        [Min(0f)]
        private float healValue;

        public float HealValue => healValue;
    }
}