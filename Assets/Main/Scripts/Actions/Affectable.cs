using Main.Scripts.Effects;

namespace Main.Scripts.Actions
{
    public interface Affectable
    {
        void AddEffects(EffectsCombination effectsCombination);
    }
}