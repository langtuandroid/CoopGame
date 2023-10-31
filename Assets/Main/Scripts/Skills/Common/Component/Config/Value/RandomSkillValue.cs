using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
[CreateAssetMenu(fileName = "RandomSkillValue", menuName = "Skill/Value/Random")]
public class RandomSkillValue : SkillValue
{
    [SerializeField]
    private int min;
    [SerializeField]
    private int max;

    public int Min => min;
    public int Max => max;
}
}