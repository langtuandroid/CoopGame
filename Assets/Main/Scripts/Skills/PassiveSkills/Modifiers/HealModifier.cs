using Main.Scripts.Actions;

namespace Main.Scripts.Skills.PassiveSkills.Modifiers
{
    public interface HealModifier
    {
        void ApplyHeal(Healable healableObject);
    }
}