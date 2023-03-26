namespace Main.Scripts.Actions.Health
{
    public interface Damageable : HealthProvider
    {
        void ApplyDamage(float damage);
    }
}