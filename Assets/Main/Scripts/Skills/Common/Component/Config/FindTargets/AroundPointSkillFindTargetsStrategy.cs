using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    [CreateAssetMenu(fileName = "AroundPointTargets", menuName = "Skill/FindTargets/AroundPoint")]
    public class AroundPointSkillFindTargetsStrategy : SkillFindTargetsStrategyBase
    {
        [SerializeField]
        private SkillPointType originPoint;
        [SerializeField]
        [Min(0f)]
        private float radius = 1f;
        [SerializeField]
        private SkillTargetType targetType;

        public SkillPointType OriginPoint => originPoint;
        public float Radius => radius;
        public SkillTargetType TargetType => targetType;
    }
}