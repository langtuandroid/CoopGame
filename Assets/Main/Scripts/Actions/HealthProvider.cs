namespace Main.Scripts.Actions
{
    public interface HealthProvider
    {
        uint GetMaxHealth();
        uint GetCurrentHealth();
    }
}