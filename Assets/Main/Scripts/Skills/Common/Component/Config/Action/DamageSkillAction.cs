using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "DamageAction", menuName = "Skill/Action/Damage")]
    public class DamageSkillAction : SkillActionBase
    {
        [SerializeField]
        private SkillValue damageValue = null!;

        public SkillValue DamageValue => damageValue;
        
        private void OnValidate()
        {
            Assert.IsTrue(damageValue != null, $"{name}: Damage value must be not null");
        }
    }
}