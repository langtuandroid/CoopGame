using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "DamageAction", menuName = "Skill/Action/Damage")]
    public class DamageSkillAction : SkillActionBase
    {
        [SerializeField]
        [Min(0f)]
        private float damageValue;

        public float DamageValue => damageValue;
    }
}