using UnityEngine;

namespace Main.Scripts.Mobs.Config.UnitState.Check
{
    [CreateAssetMenu(fileName = "CurrentHealthCheck", menuName = "Mobs/LogicBlocks/UnitStateCheck/CurrentHealth")]
    public class CurrentHealthStateCheck : UnitStateCheckBase
    {
        [SerializeField]
        [Range(0f, 100f)]
        private float percent;

        public float Percent => percent;
    }
}