using Main.Scripts.Effects.Stats;

namespace Main.Scripts.Effects
{
    public interface EffectDataChanger
    {
        public void UpdateEffectData(int effectId, ref ActiveEffectData activeEffectData, bool isUnlimitedEffect);
        
        public void RemoveLimitedEffectData(int effectId);

        public void UpdateStatAdditiveSum(StatType statType, float constValue, float percentValue);

        public void ResetAllEffectData();

        public void SetEffectDataChangeListener(EffectDataChangeListener? effectDataChangeListener);
    }
}