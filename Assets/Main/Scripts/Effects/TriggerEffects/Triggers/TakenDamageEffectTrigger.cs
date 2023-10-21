using UnityEngine;

namespace Main.Scripts.Effects.TriggerEffects.Triggers
{
    [CreateAssetMenu(fileName = "TakenDamageTrigger", menuName = "Skill/Effects/Triggers/TakenDamageTrigger")]
    public class TakenDamageEffectTrigger : EffectTriggerBase
    {
        [SerializeField]
        [Min(0f)]
        private float minDamageValue;

        public float MinDamageValue => minDamageValue;
    }
}