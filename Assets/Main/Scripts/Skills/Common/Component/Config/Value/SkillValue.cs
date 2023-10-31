using Main.Scripts.Skills.Common.Component.Config.Value.Resolver;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
public abstract class SkillValue : ScriptableObject
{
    public float Resolve(SkillVariableProvider variableProvider, GameObject? target = null)
    {
        return SkillValueResolver.Resolve(this, variableProvider, target);
    }
}
}