using System;

namespace Main.Scripts.Skills.Common.Controller.Interruption
{
[Serializable]
public struct SkillInterruptionData
{
    public bool BySelectedTargetDeath;
    public bool ByCancel;
    public bool ByAnotherSkillActivation;
}
}