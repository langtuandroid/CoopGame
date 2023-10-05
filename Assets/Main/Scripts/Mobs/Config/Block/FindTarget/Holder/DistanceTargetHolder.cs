using UnityEngine;

namespace Main.Scripts.Mobs.Config.Block.FindTarget
{
    [CreateAssetMenu(fileName = "DistanceTargetHolder", menuName = "Mobs/LogicBlocks/TargetHolder/DistanceHolder")]
    public class DistanceTargetHolder : FindTargetHolderConfigBase
    {
        [SerializeField]
        [Min(0f)]
        private float distance;

        public float Distance => distance;
    }
}