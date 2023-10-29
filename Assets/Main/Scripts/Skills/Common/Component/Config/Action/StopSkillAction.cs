using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "StopAction", menuName = "Skill/Action/Stop")]
    public class StopSkillAction : SkillActionBase
    {
        [SerializeField]
        [Min(1)]
        private int liveUntilTriggersCount = 1;
        [SerializeField]
        private bool stopControllerExecution;

        public int LiveUntilTriggersCount => liveUntilTriggersCount;
        public bool StopControllerExecution => stopControllerExecution;
    }
}