using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    [CreateAssetMenu(fileName = "MoveToTarget", menuName = "Skill/Follow/MoveTo")]
    public class MoveToTargetSkillFollowStrategy : SkillFollowStrategyBase
    {
        [SerializeField]
        private SkillPointType moveTo;
        [SerializeField]
        [Min(0f)]
        private float speed;

        public SkillPointType MoveTo => moveTo;
        public float Speed => speed;
    }
}