using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    [CreateAssetMenu(fileName = "CollisionTrigger", menuName = "Skill/Trigger/Collision")]
    public class CollisionSkillActionTrigger : SkillActionTriggerBase
    {
        [SerializeField]
        private SkillValue radius = null!;
        [SerializeField]
        private bool isAffectTargetsOnlyOneTime;
        [SerializeField]
        private UnitTargetType targetType;
        [SerializeField]
        private LayerMask triggerByDecorationsLayer;

        public SkillValue Radius => radius;
        public bool IsAffectTargetsOnlyOneTime => isAffectTargetsOnlyOneTime;
        public UnitTargetType TargetType => targetType;
        public LayerMask TriggerByDecorationsLayer => triggerByDecorationsLayer;

        private void OnValidate()
        {
            Assert.IsTrue(radius != null, $"{name}: Radius value must be not null");
        }
    }
}