using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    [CreateAssetMenu(fileName = "AttachToTarget", menuName = "Skill/Follow/AttachToTarget")]
    public class AttachToTargetSkillFollowStrategy : SkillFollowStrategyBase
    {
        [SerializeField]
        private SkillPointType attachTo;

        public SkillPointType AttachTo => attachTo;
    }
}