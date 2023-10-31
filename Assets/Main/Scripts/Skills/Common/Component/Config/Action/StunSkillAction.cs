using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "StunAction", menuName = "Skill/Action/Stun")]
    public class StunSkillAction : SkillActionBase
    {
        [SerializeField]
        private SkillValue durationTicks = null!;

        public SkillValue DurationTicks => durationTicks;
        
        private void OnValidate()
        {
            Assert.IsTrue(durationTicks != null, $"{name}: Duration Ticks value must be not null");
        }
    }
}