using Main.Scripts.Player.InputSystem.Target;
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
        private float directionAngleOffset;
        [SerializeField]
        [Min(0f)]
        private float radius = 1f;
        [SerializeField]
        [Min(0f)]
        private float angle;
        [SerializeField]
        private UnitTargetType targetType;

        public SkillPointType OriginPoint => originPoint;
        public SkillDirectionType DirectionType => directionType;
        public float DirectionAngleOffset => directionAngleOffset;
        public float Radius => radius;
        public float Angle => angle;
        public UnitTargetType TargetType => targetType;
    }
}