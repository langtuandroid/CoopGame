using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value.Resolver
{
public interface SkillVariableProvider
{
    public int GetValue(SkillVariableType variableType, GameObject? target);
    public Vector3 GetPointByType(SkillPointType pointType, GameObject? foundTarget = null);
}
}