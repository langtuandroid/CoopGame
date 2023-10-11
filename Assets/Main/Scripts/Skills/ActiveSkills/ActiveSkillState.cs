namespace Main.Scripts.Skills.ActiveSkills
{
    public enum ActiveSkillState
    {
        NotAttacking,
        Casting,
        Attacking,
        WaitingForPoint,
        WaitingForTarget,
        WaitingForPowerCharge,
        Finished,
        Canceled,
    }
}