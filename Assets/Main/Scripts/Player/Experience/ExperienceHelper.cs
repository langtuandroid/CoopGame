namespace Main.Scripts.Player.Experience
{
    public static class ExperienceHelper
    {
        public const uint MAX_LEVEL = 25;

        public static uint GetExperienceForNextLevel(uint currentLevel)
        {
            return currentLevel switch
            {
                1 => 200,
                2 => 400,
                3 => 800,
                4 => 1_000,
                5 => 1_500,
                6 => 2_000,
                7 => 2_500,
                8 => 3_000,
                9 => 3_500,
                10 => 4_000,
                11 => 6_000,
                12 => 8_000,
                13 => 10_000,
                14 => 12_000,
                15 => 16_000,
                16 => 20_000,
                17 => 24_000,
                18 => 28_000,
                19 => 32_000,
                20 => 40_000,
                21 => 48_000,
                22 => 56_000,
                23 => 60_000,
                24 => 64_000,
                _ => 0
            };
        }

        public static uint GetMaxSkillPointsByLevel(uint level)
        {
            return level switch
            {
                1 => 3,
                2 => 4,
                3 => 5,
                4 => 6,
                5 => 8,
                6 => 10,
                7 => 12,
                8 => 14,
                9 => 16,
                10 => 19,
                11 => 22,
                12 => 25,
                13 => 28,
                14 => 31,
                15 => 35,
                16 => 38,
                17 => 41,
                18 => 44,
                19 => 47,
                20 => 51,
                21 => 55,
                22 => 59,
                23 => 63,
                24 => 67,
                25 => 73,
                _ => 0
            };
        }
    }
}