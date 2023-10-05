using UnityEngine;

namespace Main.Scripts.Mobs.Config.Condition
{
    [CreateAssetMenu(fileName = "TimerCondition", menuName = "Mobs/LogicBlocks/Condition/Timer")]
    public class TimerMobCondition : MobConditionConfigBase
    {
        [SerializeField]
        [Min(0)]
        private int durationTicks;

        public int DurationTicks => durationTicks;
    }
}