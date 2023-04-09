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
        private SkillTargetType targetType;

        public float Radius => radius;
        public SkillTargetType TargetType => targetType;
    }
}