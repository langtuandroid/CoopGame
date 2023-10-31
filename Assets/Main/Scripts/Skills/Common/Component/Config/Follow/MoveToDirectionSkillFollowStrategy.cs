using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    [CreateAssetMenu(fileName = "MoveToDirectionFollowStrategy", menuName = "Skill/Follow/MoveToDirection")]
    public class MoveToDirectionSkillFollowStrategy : SkillFollowStrategyBase
    {
        [SerializeField]
        private SkillDirectionType moveDirectionType;
        [SerializeField]
        private SkillValue directionAngleOffset = null!;
        [SerializeField]
        private SkillValue speed = null!;

        public SkillDirectionType MoveDirectionType => moveDirectionType;
        public SkillValue DirectionAngleOffset => directionAngleOffset;
        public SkillValue Speed => speed;
        
        private void OnValidate()
        {
            Assert.IsTrue(directionAngleOffset != null, $"{name}: Direction Angle Offset value must be not null");
            Assert.IsTrue(speed != null, $"{name}: Speed value must be not null");
        }
    }
}