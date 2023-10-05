using System;
using Main.Scripts.Mobs.Config.Condition;
using UnityEngine.Pool;

namespace Main.Scripts.Mobs.Component.Delegate.Condition
{
    public static class ConditionDelegateHelper
    {
        public static ConditionDelegate Create(MobConditionConfigBase conditionConfig)
        {
            switch (conditionConfig)
            {
                case CanActivateSkillMobCondition canActivateSkillCondition:
                    var canActivateSkillConditionDelegate = GenericPool<CanActivateSkillConditionDelegate>.Get();
                    canActivateSkillConditionDelegate.Init(canActivateSkillCondition);
                    return canActivateSkillConditionDelegate;
                case DistanceToTargetMobCondition distanceToTargetMobCondition:
                    var distanceConditionDelegate = GenericPool<DistanceToTargetConditionDelegate>.Get();
                    distanceConditionDelegate.Init(distanceToTargetMobCondition);
                    return distanceConditionDelegate;
                case HasTargetMobCondition hasTargetMobCondition:
                    var hasTargetConditionDelegate = GenericPool<HasTargetConditionDelegate>.Get();
                    hasTargetConditionDelegate.Init(hasTargetMobCondition);
                    return hasTargetConditionDelegate;
                case TimerMobCondition timerMobCondition:
                    var timerConditionDelegate = GenericPool<TimerConditionDelegate>.Get();
                    timerConditionDelegate.Init(timerMobCondition);
                    return timerConditionDelegate;
                case UnitStateMobConditionBase unitStateMobConditionBase:
                    var unitStateConditionDelegate = GenericPool<UnitStateConditionDelegate>.Get();
                    unitStateConditionDelegate.Init(unitStateMobConditionBase);
                    return unitStateConditionDelegate;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionConfig), conditionConfig, null);
            }
        }

        public static void Release(ConditionDelegate conditionDelegate)
        {
            conditionDelegate.Reset();
            switch (conditionDelegate)
            {
                case CanActivateSkillConditionDelegate canActivateSkillConditionDelegate:
                    GenericPool<CanActivateSkillConditionDelegate>.Release(canActivateSkillConditionDelegate);
                    break;
                case DistanceToTargetConditionDelegate distanceToTargetConditionDelegate:
                    GenericPool<DistanceToTargetConditionDelegate>.Release(distanceToTargetConditionDelegate);
                    break;
                case HasTargetConditionDelegate hasTargetConditionDelegate:
                    GenericPool<HasTargetConditionDelegate>.Release(hasTargetConditionDelegate);
                    break;
                case TimerConditionDelegate timerConditionDelegate:
                    GenericPool<TimerConditionDelegate>.Release(timerConditionDelegate);
                    break;
                case UnitStateConditionDelegate unitStateConditionDelegate:
                    GenericPool<UnitStateConditionDelegate>.Release(unitStateConditionDelegate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionDelegate), conditionDelegate, null);
            }
        }
    }
}