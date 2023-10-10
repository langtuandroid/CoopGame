using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "AddChargeAction", menuName = "Skill/Action/AddCharge")]
    public class AddChargeAction : SkillActionBase
    {
        [SerializeField]
        [Min(0)]
        private int chargeValue;

        public int ChargeValue => chargeValue;
    }
}