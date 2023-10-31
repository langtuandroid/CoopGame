using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
[CreateAssetMenu(fileName = "SumValue", menuName = "Skill/Value/Sum")]
public class SumSkillValue : SkillValue
{
    [SerializeField]
    private SkillValue valueA = null!;
    [SerializeField]
    private SkillValue valueB = null!;

    public SkillValue ValueA => valueA;
    public SkillValue ValueB => valueB;
}
}