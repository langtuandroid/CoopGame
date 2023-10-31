using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "ForceAction", menuName = "Skill/Action/Force")]
    public class ForceSkillAction : SkillActionBase
    {
        [SerializeField]
        private SkillValue forceValue = null!;

        public SkillValue ForceValue => forceValue;

        private void OnValidate()
        {
            Assert.IsTrue(forceValue != null, $"{name}: Force value must be not null");
        }
    }
}