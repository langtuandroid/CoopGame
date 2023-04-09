using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "StunAction", menuName = "Skill/Action/Stun")]
    public class StunSkillAction : SkillActionBase
    {
        [SerializeField]
        [Min(0f)]
        private float durationSec;

        public float DurationSec => durationSec;
    }
}