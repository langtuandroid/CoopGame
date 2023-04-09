namespace Main.Scripts.Utils
{
    public static class TickHelper
    {
        public static bool CheckFrequency(int tick, int tickRate, float frequency)
        {
            return tick % (int)(tickRate / frequency) == 0;
        }
    }
}