using Main.Scripts.Actions;

namespace Main.Scripts.Skills.PassiveSkills.Effects.Damage
{
    public interface DamageEffect : Effect
    {
        void ApplyDamage(Damageable damageableObject);
    }
}