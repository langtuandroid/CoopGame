using System;

namespace Main.Scripts.Skills.Common.Controller.Interruption
{
[Serializable]
public struct SkillInterruptionData
{
    public bool BySelectedTargetDeath;
    public bool ByCancel;
    public bool ByAnotherSkillActivation;

    public static SkillInterruptionData DEFAULT = new()
    {
        BySelectedTargetDeath = true
    };
}
}