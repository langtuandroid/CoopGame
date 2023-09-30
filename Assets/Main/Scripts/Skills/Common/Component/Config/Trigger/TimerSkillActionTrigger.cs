using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    [CreateAssetMenu(fileName = "TimerTrigger", menuName = "Skill/Trigger/Timer")]
    public class TimerSkillActionTrigger : SkillActionTriggerBase
    {
        [SerializeField]
        [Min(0)]
        private int delayTicks;

        public int DelayTicks => delayTicks;
    }
}