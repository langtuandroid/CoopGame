using Main.Scripts.Mobs.Config.Condition;

namespace Main.Scripts.Mobs.Component.Delegate.Condition
{
    public class TimerConditionDelegate : ConditionDelegate
    {
        private TimerMobCondition conditionConfig = null!;
        private int startTimerTick;

        public void Init(TimerMobCondition conditionConfig)
        {
            this.conditionConfig = conditionConfig;
        }

        public bool Check(ref MobBlockContext context)
        {
            if (context.Tick - startTimerTick > conditionConfig.DurationTicks)
            {
                startTimerTick = context.Tick;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            startTimerTick = default;
        }
    }
}