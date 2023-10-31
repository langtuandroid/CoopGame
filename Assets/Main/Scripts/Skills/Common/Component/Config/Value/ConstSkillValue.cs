using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
[CreateAssetMenu(fileName = "ConstSkillValue", menuName = "Skill/Value/Const")]
public class ConstSkillValue : SkillValue
{
    [SerializeField]
    private int constValue;

    public int ConstValue => constValue;
}
}