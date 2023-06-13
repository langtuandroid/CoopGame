using Main.Scripts.Player.InputSystem.Target;
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
        private float directionAngleOffset;
        [SerializeField]
        private float originForwardOffset;
        [SerializeField]
        [Min(0f)]
        private float length;
        [SerializeField]
        [Min(0f)]
        private float width;
        [SerializeField]
        private UnitTargetType targetType;

        public SkillPointType OriginPoint => originPoint;
        public SkillDirectionType DirectionType => directionType;
        public float DirectionAngleOffset => directionAngleOffset;
        public float OriginForwardOffset => originForwardOffset;
        public float Length => length;
        public float Width => width;
        public UnitTargetType TargetType => targetType;
    }
}