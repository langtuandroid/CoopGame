namespace Main.Scripts.Player.Experience
{
    public static class ExperienceHelper
    {
        public static int MAX_LEVEL = 25;

        public static int GetExperienceForNextLevel(int currentLevel)
        {
            switch (currentLevel)
            {
                case 1:
                    return 200;
                case 2:
                    return 400;
                case 3:
                    return 800;
                case 4:
                    return 1_000;
                case 5:
                    return 1_500;
                case 6:
                    return 2_000;
                case 7:
                    return 2_500;
                case 8:
                    return 3_000;
                case 9:
                    return 3_500;
                case 10:
                    return 4_000;
                case 11:
                    return 6_000;
                case 12:
                    return 8_000;
                case 13:
                    return 10_000;
                case 14:
                    return 12_000;
                case 15:
                    return 16_000;
                case 16:
                    return 20_000;
                case 17:
                    return 24_000;
                case 18:
                    return 28_000;
                case 19:
                    return 32_000;
                case 20:
                    return 40_000;
                case 21:
                    return 48_000;
                case 22:
                    return 56_000;
                case 23:
                    return 60_000;
                case 24:
                    return 64_000;
                default:
                    return 0;
            }
        }

        public static int GetMaxSkillPointsByLevel(int level)
        {
            switch (level)
            {
                case 1:
                    return 3;
                case 2:
                    return 4;
                case 3:
                    return 5;
                case 4:
                    return 6;
                case 5:
                    return 8;
                case 6:
                    return 10;
                case 7:
                    return 12;
                case 8:
                    return 14;
                case 9:
                    return 16;
                case 10:
                    return 19;
                case 11:
                    return 22;
                case 12:
                    return 25;
                case 13:
                    return 28;
                case 14:
                    return 31;
                case 15:
                    return 35;
                case 16:
                    return 38;
                case 17:
                    return 41;
                case 18:
                    return 44;
                case 19:
                    return 47;
                case 20:
                    return 51;
                case 21:
                    return 55;
                case 22:
                    return 59;
                case 23:
                    return 63;
                case 24:
                    return 67;
                case 25:
                    return 73;
                default:
                    return 0;
            }
        }
    }
}