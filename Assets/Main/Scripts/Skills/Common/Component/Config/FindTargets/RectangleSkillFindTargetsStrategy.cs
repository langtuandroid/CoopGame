using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    [CreateAssetMenu(fileName = "RectangleTargetsStrategy", menuName = "Skill/FindTargets/Rectangle")]
    public class RectangleSkillFindTargetsStrategy : SkillFindTargetsStrategyBase
    {
        [SerializeField]
        private SkillPointType originPoint;
        [SerializeField]
        private SkillDirectionType directionType;
        [SerializeField]
        private SkillValue directionAngleOffset = null!;
        [SerializeField]
        private SkillValue originForwardOffset = null!;
        [SerializeField]
        private SkillValue length = null!;
        [SerializeField]
        private SkillValue width = null!;
        [SerializeField]
        private UnitTargetType targetType;

        public SkillPointType OriginPoint => originPoint;
        public SkillDirectionType DirectionType => directionType;
        public SkillValue DirectionAngleOffset => directionAngleOffset;
        public SkillValue OriginForwardOffset => originForwardOffset;
        public SkillValue Length => length;
        public SkillValue Width => width;
        public UnitTargetType TargetType => targetType;
        
        private void OnValidate()
        {
            Assert.IsTrue(directionAngleOffset != null, $"{name}: Direction Angle Offset value must be not null");
            Assert.IsTrue(originForwardOffset != null, $"{name}: Origin Forward Offset value must be not null");
            Assert.IsTrue(length != null, $"{name}: Length value must be not null");
            Assert.IsTrue(width != null, $"{name}: Width value must be not null");
        }
    }
}