using Fusion;
using Main.Scripts.Mobs.Config.Block.FindTarget;

namespace Main.Scripts.Mobs.Component.Delegate.FindTarget.Holder
{
    public class TimerTargetHolderDelegate : FindTargetHolderDelegate
    {
        private TimerTargetHolder holderConfig = null!;
        private int startTimerTick;
        private NetworkObject? target;

        public void Init(TimerTargetHolder holderConfig)
        {
            this.holderConfig = holderConfig;
        }

        public NetworkObject? GetHoldTarget(ref MobBlockContext context)
        {
            if (target != null && context.Tick - startTimerTick < holderConfig.DurationTicks)
            {
                return target;
            }

            return null;
        }

        public void SetTarget(NetworkObject target)
        {
            this.target = target;
        }

        public void Reset()
        {
            holderConfig = null!;
            startTimerTick = default;
        }
    }
}