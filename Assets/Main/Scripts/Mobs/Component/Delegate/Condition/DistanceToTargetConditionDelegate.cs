using Main.Scripts.Mobs.Config.Condition;
using UnityEngine;

namespace Main.Scripts.Mobs.Component.Delegate.Condition
{
    public class DistanceToTargetConditionDelegate : ConditionDelegate
    {
        private DistanceToTargetMobCondition conditionConfig = null!;

        public void Init(DistanceToTargetMobCondition conditionConfig)
        {
            this.conditionConfig = conditionConfig;
        }

        public bool Check(ref MobBlockContext context)
        {
            return context.TargetUnit != null
                   && Vector3.Distance(
                       context.SelfUnit.transform.position,
                       context.TargetUnit.transform.position
                   ) < conditionConfig.Distance;
        }

        public void Reset()
        {
            conditionConfig = null!;
        }
    }
}