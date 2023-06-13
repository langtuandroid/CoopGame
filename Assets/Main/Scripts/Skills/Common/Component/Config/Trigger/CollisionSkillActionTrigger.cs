using Main.Scripts.Player.InputSystem.Target;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    [CreateAssetMenu(fileName = "CollisionTrigger", menuName = "Skill/Trigger/Collision")]
    public class CollisionSkillActionTrigger : SkillActionTriggerBase
    {
        [SerializeField]
        [Min(0f)]
        private float radius;
        [SerializeField]
        private UnitTargetType targetType;

        public float Radius => radius;
        public UnitTargetType TargetType => targetType;
    }
}