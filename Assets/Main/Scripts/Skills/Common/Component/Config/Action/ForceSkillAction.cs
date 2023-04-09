using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "ForceAction", menuName = "Skill/Action/Force")]
    public class ForceSkillAction : SkillActionBase
    {
        [SerializeField]
        [Min(0f)]
        private float forceValue;

        public float ForceValue => forceValue;
    }
}