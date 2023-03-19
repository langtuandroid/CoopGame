using Main.Scripts.Actions;

namespace Main.Scripts.Skills.PassiveSkills.Modifiers
{
    public interface DamageableModifier
    {
        void ApplyDamage(Damageable damageableObject);
    }
}