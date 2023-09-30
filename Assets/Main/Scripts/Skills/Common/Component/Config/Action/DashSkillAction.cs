using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "DashAction", menuName = "Skill/Action/Dash")]
    public class DashSkillAction : SkillActionBase
    {
        [SerializeField]
        private SkillDirectionType directionType;
        [SerializeField]
        private float speed;
        [SerializeField]
        [Min(0)]
        private int durationTicks;

        public SkillDirectionType DirectionType => directionType;
        public float Speed => speed;
        public int DurationTicks => durationTicks;
    }
}