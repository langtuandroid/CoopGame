using System;
using Main.Scripts.Mobs.Config.Block.FindTarget;
using UnityEngine.Pool;

namespace Main.Scripts.Mobs.Component.Delegate.FindTarget.Holder
{
    public static class FindTargetHolderDelegateHelper
    {
        public static FindTargetHolderDelegate Create(FindTargetHolderConfigBase holderConfig)
        {
            switch (holderConfig)
            {
                case DistanceTargetHolder distanceHolderConfig:
                    var distanceTargetHolder = GenericPool<DistanceTargetHolderDelegate>.Get();
                    distanceTargetHolder.Init(distanceHolderConfig);
                    return distanceTargetHolder;
                case TimerTargetHolder timerHolderConfig:
                    var timerTargetHolder = GenericPool<TimerTargetHolderDelegate>.Get();
                    timerTargetHolder.Init(timerHolderConfig);
                    return timerTargetHolder;
                default:
                    throw new ArgumentOutOfRangeException(nameof(holderConfig), holderConfig, null);
            }
        }

        public static void Release(FindTargetHolderDelegate holderDelegate)
        {
            holderDelegate.Reset();
            switch (holderDelegate)
            {
                case DistanceTargetHolderDelegate distanceHolderDelegate:
                    GenericPool<DistanceTargetHolderDelegate>.Release(distanceHolderDelegate);
                    break;
                case TimerTargetHolderDelegate timerHolderDelegate:
                    GenericPool<TimerTargetHolderDelegate>.Release(timerHolderDelegate);
                    break;
            }
        }
    }
}