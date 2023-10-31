using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
[CreateAssetMenu(fileName = "RandomFloatValue", menuName = "Skill/Value/RandomFloat")]
public class RandomFloatSkillValue : SkillValue
{
    [SerializeField]
    private float min;
    [SerializeField]
    private float max;

    public float Min => min;
    public float Max => max;
}
}