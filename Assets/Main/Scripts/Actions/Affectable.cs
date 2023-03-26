using Main.Scripts.Effects;

namespace Main.Scripts.Actions
{
    public interface Affectable
    {
        void ApplyEffects(EffectsCombination effectsCombination);
    }
}