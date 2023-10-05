using Main.Scripts.Mobs.Config.Condition;
using UnityEngine;

namespace Main.Scripts.Mobs.Config.Block.Branch
{
    [CreateAssetMenu(fileName = "BranchBlock", menuName = "Mobs/LogicBlocks/Branch")]
    public class BranchMobBlockConfig : MobBlockConfigBase
    {
        [SerializeField]
        private MobConditionConfigBase condition = null!;
        [SerializeField]
        private MobBlockConfigBase blockIfConditionTrue = null!;
        [SerializeField]
        private MobBlockConfigBase blockIfConditionFalse = null!;

        public MobConditionConfigBase Condition => condition;
        public MobBlockConfigBase BlockIfConditionTrue => blockIfConditionTrue;
        public MobBlockConfigBase BlockIfConditionFalse => blockIfConditionFalse;
    }
}