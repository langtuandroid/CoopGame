using Main.Scripts.Actions;

namespace Main.Scripts.Skills.PassiveSkills.Effects.Heal
{
    public interface HealEffect : Effect
    {
        void ApplyHeal(Healable healableObject);
    }
}