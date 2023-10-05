using Main.Scripts.Mobs.Config.UnitState.Check;
using UnityEngine;

namespace Main.Scripts.Mobs.Config.Condition
{
    [CreateAssetMenu(fileName = "UnitStateCondition", menuName = "Mobs/LogicBlocks/Condition/UnitState")]
    public abstract class UnitStateMobConditionBase : MobConditionConfigBase
    {
        [SerializeField]
        private MobUnitTarget unitTarget;
        [SerializeField]
        private UnitStateCheckBase stateCheck;

        public MobUnitTarget UnitTarget => unitTarget;
        public UnitStateCheckBase StateCheck => stateCheck;
    }
}