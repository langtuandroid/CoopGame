using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
[CreateAssetMenu(fileName = "RandomIntValue", menuName = "Skill/Value/RandomInt")]
public class RandomIntSkillValue : SkillValue
{
    [SerializeField]
    private int min;
    [SerializeField]
    private int max;

    public int Min => min;
    public int Max => max;
}
}