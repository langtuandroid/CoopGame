using Main.Scripts.Mobs.Component.Delegate.Condition;
using Main.Scripts.Mobs.Config.Block.Branch;

namespace Main.Scripts.Mobs.Component.Delegate.Branch
{
    public class BranchBlockDelegate : MobBlockDelegate
    {
        private ConditionDelegate conditionDelegate = null!;
        private MobBlockDelegate blockIfConditionTrueDelegate = null!;
        private MobBlockDelegate blockIfConditionFalseDelegate = null!;

        public void Init(BranchMobBlockConfig branchConfig)
        {
            conditionDelegate = ConditionDelegateHelper.Create(branchConfig.Condition);
            blockIfConditionTrueDelegate = MobBlockDelegateHelper.Create(branchConfig.BlockIfConditionTrue);
            blockIfConditionFalseDelegate = MobBlockDelegateHelper.Create(branchConfig.BlockIfConditionFalse);
        }

        public void Reset()
        {
            ConditionDelegateHelper.Release(conditionDelegate);
            MobBlockDelegateHelper.Release(blockIfConditionTrueDelegate);
            MobBlockDelegateHelper.Release(blockIfConditionFalseDelegate);

            conditionDelegate = null!;
            blockIfConditionTrueDelegate = null!;
            blockIfConditionFalseDelegate = null!;
        }

        public void Do(ref MobBlockContext context, out MobBlockResult blockResult)
        {
            if (conditionDelegate.Check(ref context))
            {
                blockIfConditionTrueDelegate.Do(ref context, out blockResult);
            }
            else
            {
                blockIfConditionFalseDelegate.Do(ref context, out blockResult);
            }
        }
    }
}