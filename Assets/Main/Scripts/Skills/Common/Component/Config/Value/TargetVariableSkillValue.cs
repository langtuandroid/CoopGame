using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
[CreateAssetMenu(fileName = "TargetVariableValue", menuName = "Skill/Value/TargetVariable")]
public class TargetVariableSkillValue : SkillValue
{
    [SerializeField]
    private float multiplier = 1;
    [SerializeField]
    private SkillTargetVariableType variableType;
    [SerializeField]
    private SkillTargetType targetType;

    public float Multiplier => multiplier;
    public SkillTargetVariableType VariableType => variableType;
    public SkillTargetType TargetType => targetType;
}
}