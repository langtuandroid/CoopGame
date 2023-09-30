using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    [CreateAssetMenu(fileName = "MoveToDirectionFollowStrategy", menuName = "Skill/Follow/MoveToDirection")]
    public class MoveToDirectionSkillFollowStrategy : SkillFollowStrategyBase
    {
        [SerializeField]
        private SkillDirectionType moveDirectionType;
        [SerializeField]
        private float directionAngleOffset;
        [SerializeField]
        [Min(0f)]
        private float speed;

        public SkillDirectionType MoveDirectionType => moveDirectionType;
        public float DirectionAngleOffset => directionAngleOffset;
        public float Speed => speed;
    }
}