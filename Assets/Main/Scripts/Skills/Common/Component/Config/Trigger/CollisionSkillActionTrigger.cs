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
        private bool isAffectTargetsOnlyOneTime;
        [SerializeField]
        private UnitTargetType targetType;
        [SerializeField]
        private LayerMask triggerByDecorationsLayer;

        public float Radius => radius;
        public bool IsAffectTargetsOnlyOneTime => isAffectTargetsOnlyOneTime;
        public UnitTargetType TargetType => targetType;
        public LayerMask TriggerByDecorationsLayer => triggerByDecorationsLayer;
    }
}