using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    [CreateAssetMenu(fileName = "PeriodicTrigger", menuName = "Skill/Trigger/Periodic")]
    public class PeriodicSkillActionTrigger : SkillActionTriggerBase
    {
        [SerializeField]
        [Min(1)]
        private int frequencyTicks = 24;

        public int FrequencyTicks => frequencyTicks;
    }
}