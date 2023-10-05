using Fusion;

namespace Main.Scripts.Mobs.Component.Delegate.FindTarget.Holder
{
    public interface FindTargetHolderDelegate
    {
        public NetworkObject? GetHoldTarget(ref MobBlockContext context);
        public void SetTarget(NetworkObject target);
        public void Reset();
    }
}