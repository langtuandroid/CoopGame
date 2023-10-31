using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    [CreateAssetMenu(fileName = "PeriodicTrigger", menuName = "Skill/Trigger/Periodic")]
    public class PeriodicSkillActionTrigger : SkillActionTriggerBase
    {
        [SerializeField]
        private SkillValue frequencyTicks = null!;

        public SkillValue FrequencyTicks => frequencyTicks;

        private void OnValidate()
        {
            Assert.IsTrue(frequencyTicks != null, $"{name}: Frequency Ticks value must be not null");
        }
    }
}