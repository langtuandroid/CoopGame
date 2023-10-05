using Fusion;
using Main.Scripts.Mobs.Config.Block.FindTarget;
using UnityEngine;

namespace Main.Scripts.Mobs.Component.Delegate.FindTarget.Holder
{
    public class DistanceTargetHolderDelegate : FindTargetHolderDelegate
    {
        private DistanceTargetHolder holderConfig = null!;
        private NetworkObject? target;

        public void Init(DistanceTargetHolder holderConfig)
        {
            this.holderConfig = holderConfig;
        }

        public NetworkObject? GetHoldTarget(ref MobBlockContext context)
        {
            if (target != null
                && Vector3.Distance(
                    context.SelfUnit.transform.position,
                    target.transform.position
                ) <= holderConfig.Distance)
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
            target = null!;
        }
    }
}