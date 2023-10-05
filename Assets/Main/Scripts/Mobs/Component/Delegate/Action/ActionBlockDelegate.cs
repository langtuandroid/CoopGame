using Main.Scripts.Mobs.Config.Block.Action;

namespace Main.Scripts.Mobs.Component.Delegate.Action
{
    public class ActionBlockDelegate : MobBlockDelegate
    {
        private ActionMobBlockConfigBase actionConfig = null!;

        public void Init(ActionMobBlockConfigBase actionConfig)
        {
            this.actionConfig = actionConfig;
        }

        public void Do(ref MobBlockContext context, out MobBlockResult blockResult)
        {
            blockResult.blockContext = context;
            blockResult.actionConfig = actionConfig;
        }

        public void Reset()
        {
            actionConfig = null!;
        }
    }
}