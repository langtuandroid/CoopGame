using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    [CreateAssetMenu(fileName = "CircleSectorTargetsStrategy", menuName = "Skill/FindTargets/CircleSector")]
    public class CircleSectorSkillFindTargetsStrategy : SkillFindTargetsStrategyBase
    {
        [SerializeField]
        private SkillPointType originPoint;
        [SerializeField]
        private SkillDirectionType directionType;
        [SerializeField]
        private SkillValue directionAngleOffset = null!;
        [SerializeField]
        private SkillValue radius = null!;
        [SerializeField]
        private SkillValue angle = null!;
        [SerializeField]
        private UnitTargetType targetType;

        public SkillPointType OriginPoint => originPoint;
        public SkillDirectionType DirectionType => directionType;
        public SkillValue DirectionAngleOffset => directionAngleOffset;
        public SkillValue Radius => radius;
        public SkillValue Angle => angle;
        public UnitTargetType TargetType => targetType;
        
        private void OnValidate()
        {
            Assert.IsTrue(directionAngleOffset != null, $"{name}: Direction Angle Offset value must be not null");
            Assert.IsTrue(radius != null, $"{name}: Radius value must be not null");
            Assert.IsTrue(angle != null, $"{name}: Angle value must be not null");
        }
    }
}