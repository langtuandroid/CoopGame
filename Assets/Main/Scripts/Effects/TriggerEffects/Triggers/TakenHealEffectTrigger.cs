using UnityEngine;

namespace Main.Scripts.Effects.TriggerEffects.Triggers
{
    [CreateAssetMenu(fileName = "TakenHealTrigger", menuName = "Skill/Effects/Triggers/TakenHealTrigger")]
    public class TakenHealEffectTrigger : EffectTriggerBase
    {
        [SerializeField]
        [Min(0f)]
        private float minHealValue;

        public float MinHealValue => minHealValue;
    }
}