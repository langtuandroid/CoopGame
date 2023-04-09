namespace Main.Scripts.Skills.Common.Component.Config
{
    public enum SkillInterruptStrategy
    {
        NoInterruptible,
        InterruptOnUsingOtherSkill,
        InterruptOnSelectedUnitDeath,
        InterruptOnSelfUnitDeath
    }
}