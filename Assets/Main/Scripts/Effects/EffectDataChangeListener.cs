using Main.Scripts.Effects.Stats;

namespace Main.Scripts.Effects
{
    public interface EffectDataChangeListener
    {
        public void OnUpdateEffectData(int effectId, ref ActiveEffectData activeEffectData, bool isUnlimitedEffect);
        
        public void OnRemoveLimitedEffectData(int effectId);

        public void OnUpdateStatAdditiveSum(StatType statType, float constValue, float percentValue);

        public void OnResetAllEffectData();
    }
}