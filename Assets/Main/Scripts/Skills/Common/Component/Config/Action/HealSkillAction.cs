using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "HealAction", menuName = "Skill/Action/Heal")]
    public class HealSkillAction : SkillActionBase
    {
        [SerializeField]
        private SkillValue healValue = null!;

        public SkillValue HealValue => healValue;
        
        private void OnValidate()
        {
            Assert.IsTrue(healValue != null, $"{name}: Heal value must be not null");
        }
    }
}