using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
[CreateAssetMenu(fileName = "CommonVariableValue", menuName = "Skill/Value/CommonVariable")]
public class CommonVariableSkillValue : SkillValue
{
    [SerializeField]
    private int multiplier = 1;
    [SerializeField]
    private SkillCommonVariableType variableType;

    public int Multiplier => multiplier;
    public SkillCommonVariableType VariableType => variableType;
}
}