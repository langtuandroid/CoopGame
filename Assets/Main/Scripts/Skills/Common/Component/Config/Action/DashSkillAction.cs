using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "DashAction", menuName = "Skill/Action/Dash")]
    public class DashSkillAction : SkillActionBase
    {
        [SerializeField]
        private SkillDirectionType directionType;
        [SerializeField]
        private SkillValue speed = null!;
        [SerializeField]
        private SkillValue durationTicks = null!;

        public SkillDirectionType DirectionType => directionType;
        public SkillValue Speed => speed;
        public SkillValue DurationTicks => durationTicks;
        
        private void OnValidate()
        {
            Assert.IsTrue(speed != null, $"{name}: Speed value must be not null");
            Assert.IsTrue(durationTicks != null, $"{name}: Duration Ticks value must be not null");
        }
    }
}