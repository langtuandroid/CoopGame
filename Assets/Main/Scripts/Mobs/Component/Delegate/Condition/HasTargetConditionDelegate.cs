using Main.Scripts.Mobs.Config.Condition;

namespace Main.Scripts.Mobs.Component.Delegate.Condition
{
    public class HasTargetConditionDelegate : ConditionDelegate
    {
        private HasTargetMobCondition conditionConfig = null!;

        public void Init(HasTargetMobCondition conditionConfig)
        {
            this.conditionConfig = conditionConfig;
        }

        public bool Check(ref MobBlockContext context)
        {
            return context.TargetUnit != null;
        }

        public void Reset()
        {
            conditionConfig = null!;
        }
    }
}