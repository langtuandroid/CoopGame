using Main.Scripts.Mobs.Component.Delegate.UnitState;
using Main.Scripts.Mobs.Config.Condition;
using Main.Scripts.Mobs.Config.UnitState.Check;

namespace Main.Scripts.Mobs.Component.Delegate.Condition
{
    public class UnitStateConditionDelegate : ConditionDelegate
    {
        private MobUnitTarget unitTargetType;
        private UnitStateCheckBase stateCheckConfig = null!;

        public void Init(UnitStateMobConditionBase conditionConfig)
        {
            unitTargetType = conditionConfig.UnitTarget;
            stateCheckConfig = conditionConfig.StateCheck;
        }

        public bool Check(ref MobBlockContext context)
        {
            var stateCheckTarget =
                unitTargetType == MobUnitTarget.SELF ? context.SelfUnit : context.TargetUnit;
            return stateCheckTarget != null && UnitStateCheckHelper.CheckState(stateCheckTarget, stateCheckConfig);
        }

        public void Reset()
        {
            unitTargetType = default;
            stateCheckConfig = null!;
        }
    }
}