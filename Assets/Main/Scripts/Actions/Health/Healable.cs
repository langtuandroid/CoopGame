namespace Main.Scripts.Actions.Health
{
    public interface Healable : HealthProvider
    {
        void ApplyHeal(float healValue);
    }
}