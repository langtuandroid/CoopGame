using System;
using Main.Scripts.Mobs.Component.Delegate.Action;
using Main.Scripts.Mobs.Component.Delegate.Branch;
using Main.Scripts.Mobs.Component.Delegate.FindTarget;
using Main.Scripts.Mobs.Config.Block;
using Main.Scripts.Mobs.Config.Block.Action;
using Main.Scripts.Mobs.Config.Block.Branch;
using Main.Scripts.Mobs.Config.Block.FindTarget;
using UnityEngine.Pool;

namespace Main.Scripts.Mobs.Component.Delegate
{
    public static class MobBlockDelegateHelper
    {
        public static MobBlockDelegate Create(MobBlockConfigBase blockConfig)
        {
            switch (blockConfig)
            {
                case BranchMobBlockConfig branchMobBlockConfig:
                    var branchDelegate = GenericPool<BranchBlockDelegate>.Get();
                    branchDelegate.Init(branchMobBlockConfig);
                    return branchDelegate;
                case FindTargetMobBlockConfig findTargetMobBlockConfig:
                    var findTargetDelegate = GenericPool<FindTargetBlockDelegate>.Get();
                    findTargetDelegate.Init(findTargetMobBlockConfig);
                    return findTargetDelegate;
                case ActionMobBlockConfigBase actionConfig:
                    var actionBlockDelegate = GenericPool<ActionBlockDelegate>.Get();
                    actionBlockDelegate.Init(actionConfig);
                    return actionBlockDelegate;
                default:
                    throw new ArgumentOutOfRangeException(nameof(blockConfig), blockConfig, null);
            }
        }

        public static void Release(MobBlockDelegate blockDelegate)
        {
            blockDelegate.Reset();
            switch (blockDelegate)
            {
                case BranchBlockDelegate branchBlockDelegate:
                    GenericPool<BranchBlockDelegate>.Release(branchBlockDelegate);
                    break;
                case FindTargetBlockDelegate findTargetBlockDelegate:
                    GenericPool<FindTargetBlockDelegate>.Release(findTargetBlockDelegate);
                    break;
                case ActionBlockDelegate actionBlockDelegate:
                    GenericPool<ActionBlockDelegate>.Release(actionBlockDelegate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(blockDelegate), blockDelegate, null);
            }
        }
    }
}