using UnityEngine;

namespace Main.Scripts.Mobs.Config.Condition
{
    [CreateAssetMenu(fileName = "RaycastToTargetCondition", menuName = "Mobs/LogicBlocks/Condition/RaycastToTarget")]
    public class RaycastToTargetCondition : MobConditionConfigBase
    {
        [SerializeField]
        [Min(0f)]
        private float distance;
        [SerializeField]
        [Min(0f)]
        private float width;
        [SerializeField]
        private LayerMask checkLayerMask;

        public float Distance => distance;
        public float Width => width;
        public LayerMask CheckLayerMask => checkLayerMask;
    }
}