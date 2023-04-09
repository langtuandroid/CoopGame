using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    [CreateAssetMenu(fileName = "RectangleTargets", menuName = "Skill/FindTargets/Rectangle")]
    public class RectangleSkillFindTargetsStrategy : SkillFindTargetsStrategyBase
    {
        [SerializeField]
        private SkillPointType originPoint;
        [SerializeField]
        private SkillDirectionType directionType;
        [SerializeField]
        private float originForwardOffset;
        [SerializeField]
        [Min(0f)]
        private float length;
        [SerializeField]
        [Min(0f)]
        private float width;
        [SerializeField]
        private SkillTargetType targetType;

        public SkillPointType OriginPoint => originPoint;
        public SkillDirectionType DirectionType => directionType;
        public float OriginForwardOffset => originForwardOffset;
        public float Length => length;
        public float Width => width;
        public SkillTargetType TargetType => targetType;
    }
}