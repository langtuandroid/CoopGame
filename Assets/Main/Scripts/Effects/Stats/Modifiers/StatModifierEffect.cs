using UnityEngine;

namespace Main.Scripts.Effects.Stats.Modifiers
{
    [CreateAssetMenu(fileName = "StatModifierEffect", menuName = "Scriptable/Effects/StatModifierEffect")]
    public class StatModifierEffect : EffectBase
    {
        [SerializeField]
        private StatType statType;
        [SerializeField]
        private float constAdditive;
        [SerializeField]
        private float percentAdditive;

        public StatType StatType => statType;
        public float ConstAdditive => constAdditive;
        public float PercentAdditive => percentAdditive;
    }
}