using UnityEngine;

namespace Main.Scripts.Mobs.Config.Block.FindTarget
{
    [CreateAssetMenu(fileName = "TimerTargetHolder", menuName = "Mobs/LogicBlocks/TargetHolder/TimerHolder")]
    public class TimerTargetHolder : FindTargetHolderConfigBase
    {
        [SerializeField]
        [Min(0)]
        private int durationTicks;

        public int DurationTicks => durationTicks;
    }
}