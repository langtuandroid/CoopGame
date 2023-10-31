using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value.Resolver
{
public interface SkillVariableProvider
{
    public int GetCommonValue(SkillCommonVariableType variableType, GameObject? target);
    public int GetTargetValue(SkillTargetVariableType variableType, SkillTargetType targetType, GameObject? foundTarget);
    public Vector3 GetPointByType(SkillPointType pointType, GameObject? foundTarget = null);
}
}