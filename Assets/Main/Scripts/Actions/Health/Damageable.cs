using Main.Scripts.Actions.Data;

namespace Main.Scripts.Actions.Health
{
    public interface Damageable : HealthProvider
    {
        void AddDamage(ref DamageActionData data);
    }
}