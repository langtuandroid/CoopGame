using System;

namespace Main.Scripts.Skills.Common.Controller.Interruption
{
[Flags]
public enum SkillInterruptionType
{
    OwnerDead = 1 << 0,
    OwnerStunned = 1 << 1,
    SelectedTargetDead = 1 << 2,
    AnotherSkillActivation = 1 << 3,
    Cancel = 1 << 4,
}
}