namespace Main.Scripts.Actions.Health
{
    public interface HealthProvider
    {
        float GetMaxHealth();
        float GetCurrentHealth();
    }
}