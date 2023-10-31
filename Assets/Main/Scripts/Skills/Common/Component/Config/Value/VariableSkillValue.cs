using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
[CreateAssetMenu(fileName = "VariableSkillValue", menuName = "Skill/Value/Variable")]
public class VariableSkillValue : SkillValue
{
    [SerializeField]
    private int percentValue = 100;
    [SerializeField]
    private SkillVariableType percentFromVariable;

    public int PercentValue => percentValue;
    public SkillVariableType PercentFromVariable => percentFromVariable;
}
}