using Main.Scripts.Skills.PassiveSkills;

namespace Main.Scripts.Actions
{
    public interface Damageable : HealthProvider
    {
        void ApplyDamage(uint damage);
    }
}