using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    [CreateAssetMenu(fileName = "TimerTrigger", menuName = "Skill/Trigger/Timer")]
    public class TimerSkillActionTrigger : SkillActionTriggerBase
    {
        [SerializeField]
        private SkillValue delayTicks = null!;

        public SkillValue DelayTicks => delayTicks;

        private void OnValidate()
        {
            Assert.IsTrue(delayTicks != null, $"{name}: Delay Ticks value must be not null");
        }
    }
}