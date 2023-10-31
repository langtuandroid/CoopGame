using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
[CreateAssetMenu(fileName = "ConstValue", menuName = "Skill/Value/Const")]
public class ConstSkillValue : SkillValue
{
    [SerializeField]
    private float constValue;

    public float ConstValue => constValue;
}
}