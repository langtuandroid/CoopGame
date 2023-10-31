using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Main.Scripts.Skills.Common.Component.Config.Value.Resolver
{
public static class SkillValueResolver
{
    public static float Resolve(SkillValue value, SkillVariableProvider skillVariableProvider, GameObject? target)
    {
        switch (value)
        {
            case ConstSkillValue constSkillValue:
                return constSkillValue.ConstValue;
            case SumSkillValue sumSkillValue:
                return Resolve(sumSkillValue.ValueA, skillVariableProvider, target)
                       + Resolve(sumSkillValue.ValueB, skillVariableProvider, target);
            case CommonVariableSkillValue variableSkillValue:
                return skillVariableProvider.GetCommonValue(variableSkillValue.VariableType, target) *
                       variableSkillValue.Multiplier;
            case TargetVariableSkillValue variableTargetSkillValue:
                return skillVariableProvider.GetTargetValue(
                    variableTargetSkillValue.VariableType,
                    variableTargetSkillValue.TargetType,
                    target
                ) * variableTargetSkillValue.Multiplier;
            case RandomFloatSkillValue randomFloatSkillValue:
                return Random.Range(randomFloatSkillValue.Min, randomFloatSkillValue.Max);
            case RandomIntSkillValue randomIntSkillValue:
                return Random.Range(randomIntSkillValue.Min, randomIntSkillValue.Max);
            case DistanceSkillValue distanceSkillValue:
                return Vector3.Distance(
                    skillVariableProvider.GetPointByType(distanceSkillValue.PointA),
                    skillVariableProvider.GetPointByType(distanceSkillValue.PointB)
                );
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}
}