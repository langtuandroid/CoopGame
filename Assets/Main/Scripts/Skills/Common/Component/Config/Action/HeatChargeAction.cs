using Main.Scripts.Skills.Common.Component.Config.Value;
using UnityEngine;
using UnityEngine.Assertions;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "HeatChargeAction", menuName = "Skill/Action/HeatCharge")]
    public class HeatChargeAction : SkillActionBase
    {
        [SerializeField]
        [Min(0)]
        private SkillValue chargeValue = null!;

        public SkillValue ChargeValue => chargeValue;

        private void OnValidate()
        {
            Assert.IsTrue(chargeValue != null, $"{name}: Charge value must be not null");
        }
    }
}