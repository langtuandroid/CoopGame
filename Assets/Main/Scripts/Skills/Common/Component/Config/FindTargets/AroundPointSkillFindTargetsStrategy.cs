using Main.Scripts.Player.InputSystem.Target;
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
        private UnitTargetType targetType;

        public SkillPointType OriginPoint => originPoint;
        public float Radius => radius;
        public UnitTargetType TargetType => targetType;
    }
}