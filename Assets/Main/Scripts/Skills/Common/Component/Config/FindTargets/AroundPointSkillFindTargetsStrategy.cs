using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    [CreateAssetMenu(fileName = "AroundPointTargetsStrategy", menuName = "Skill/FindTargets/AroundPoint")]
    public class AroundPointSkillFindTargetsStrategy : SkillFindTargetsStrategyBase
    {
        [SerializeField]
        private SkillPointType originPoint;
        [SerializeField]
        private SkillValue radius = null!;
        [SerializeField]
        private UnitTargetType targetType;

        public SkillPointType OriginPoint => originPoint;
        public SkillValue Radius => radius;
        public UnitTargetType TargetType => targetType;
        
        private void OnValidate()
        {
            Assert.IsTrue(radius != null, $"{name}: Radius value must be not null");
        }
    }
}