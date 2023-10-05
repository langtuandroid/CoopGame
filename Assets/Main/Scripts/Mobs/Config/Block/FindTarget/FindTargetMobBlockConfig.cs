using Main.Scripts.Mobs.Config.UnitState.Check;
using Main.Scripts.Player.InputSystem.Target;
using UnityEngine;

namespace Main.Scripts.Mobs.Config.Block.FindTarget
{
    [CreateAssetMenu(fileName = "FindTargetBlock", menuName = "Mobs/LogicBlocks/FindTarget")]
    public class FindTargetMobBlockConfig : MobBlockConfigBase
    {
        [SerializeField]
        private UnitTargetType targetType;
        [SerializeField]
        private float searchRadius;
        [SerializeField]
        private UnitStateCheckBase stateCheckFilter = null!;
        [SerializeField]
        private FindTargetSortType sortType;
        [SerializeField]
        private bool orderByDesc;
        [SerializeField]
        private FindTargetHolderConfigBase findTargetHolderConfig = null!;
        [SerializeField]
        private MobBlockConfigBase continueWithTargetContextBlock = null!;

        public UnitTargetType TargetType => targetType;
        public float SearchRadius => searchRadius;
        public UnitStateCheckBase StateCheckFilter => stateCheckFilter;
        public FindTargetSortType SortType => sortType;
        public bool OrderByDesc => orderByDesc;
        public FindTargetHolderConfigBase FindTargetHolderConfig => findTargetHolderConfig;
        public MobBlockConfigBase ContinueWithTargetContextBlock => continueWithTargetContextBlock;
    }
}