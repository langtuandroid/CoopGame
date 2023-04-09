using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    [CreateAssetMenu(fileName = "CircleSectorTargets", menuName = "Skill/FindTargets/CircleSector")]
    public class CircleSectorSkillFindTargetsStrategy : SkillFindTargetsStrategyBase
    {
        [SerializeField]
        private SkillPointType originPoint;
        [SerializeField]
        private SkillDirectionType directionType;
        [SerializeField]
        [Min(0f)]
        private float radius = 1f;
        [SerializeField]
        [Min(0f)]
        private float angle;
        [SerializeField]
        private SkillTargetType targetType;

        public SkillPointType OriginPoint => originPoint;
        public SkillDirectionType DirectionType => directionType;
        public float Radius => radius;
        public float Angle => angle;
        public SkillTargetType TargetType => targetType;
    }
}