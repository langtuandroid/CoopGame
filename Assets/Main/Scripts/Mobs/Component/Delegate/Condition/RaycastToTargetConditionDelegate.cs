using System;
using Main.Scripts.Mobs.Config.Condition;
using UnityEngine;

namespace Main.Scripts.Mobs.Component.Delegate.Condition
{
    public class RaycastToTargetConditionDelegate : ConditionDelegate
    {
        private RaycastToTargetCondition conditionConfig = null!;
        private Collider[] colliders = new Collider[2];

        public void Init(RaycastToTargetCondition conditionConfig)
        {
            this.conditionConfig = conditionConfig;
        }

        public bool Check(ref MobBlockContext context)
        {
            var target = context.TargetUnit;
            if (target == null)
            {
                throw new Exception("Target must be not null");
            }
            var deltaPosition = target.transform.position - context.SelfUnit.transform.position;
            var directionToTarget = deltaPosition.normalized;
            
            var halfDistance = Mathf.Min(conditionConfig.Distance, deltaPosition.sqrMagnitude) / 2f;

            
            var hitsCount = Physics.OverlapBoxNonAlloc(
                center: context.SelfUnit.transform.position + directionToTarget * halfDistance,
                halfExtents: new Vector3(conditionConfig.Width / 2, 1f, halfDistance),
                results: colliders,
                orientation: Quaternion.LookRotation(directionToTarget),
                mask: conditionConfig.CheckLayerMask
            );

            for (var i = 0; i < hitsCount; i++)
            {
                if (colliders[i].gameObject != context.SelfUnit.gameObject)
                {
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            conditionConfig = null!;
        }
    }
}