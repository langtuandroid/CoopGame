using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    [CreateAssetMenu(fileName = "PeriodicTrigger", menuName = "Skill/Trigger/Periodic")]
    public class PeriodicSkillActionTrigger : SkillActionTriggerBase
    {
        [SerializeField]
        private float frequency;

        public float Frequency => frequency;
    }
}