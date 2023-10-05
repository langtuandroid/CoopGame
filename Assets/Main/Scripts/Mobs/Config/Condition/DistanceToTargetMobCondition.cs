using UnityEngine;

namespace Main.Scripts.Mobs.Config.Condition
{
    [CreateAssetMenu(fileName = "DistanceToTargetCondition", menuName = "Mobs/LogicBlocks/Condition/DistanceToTarget")]
    public class DistanceToTargetMobCondition : MobConditionConfigBase
    {
        [SerializeField]
        [Min(0f)]
        private float distance;

        public float Distance => distance;
    }
}