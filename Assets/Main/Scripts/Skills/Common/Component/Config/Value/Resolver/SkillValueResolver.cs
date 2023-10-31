using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Main.Scripts.Skills.Common.Component.Config.Value.Resolver
{
public static class SkillValueResolver
{
    public static int Resolve(SkillValue value, SkillVariableProvider skillVariableProvider, GameObject? target)
    {
        switch (value)
        {
            case ConstSkillValue constSkillValue:
                return constSkillValue.ConstValue;
            case SumSkillValue sumSkillValue:
                return Resolve(sumSkillValue.ValueA, skillVariableProvider, target)
                       + Resolve(sumSkillValue.ValueB, skillVariableProvider, target);
            case VariableSkillValue variableSkillValue:
                return skillVariableProvider.GetValue(variableSkillValue.PercentFromVariable, target)
                       * variableSkillValue.PercentValue
                       / 100;
            case RandomSkillValue randomSkillValue:
                return Random.Range(randomSkillValue.Min, randomSkillValue.Max);
            case DistanceSkillValue distanceSkillValue:
                return (int)Vector3.Distance(
                    skillVariableProvider.GetPointByType(distanceSkillValue.PointA),
                    skillVariableProvider.GetPointByType(distanceSkillValue.PointB)
                );
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}
}