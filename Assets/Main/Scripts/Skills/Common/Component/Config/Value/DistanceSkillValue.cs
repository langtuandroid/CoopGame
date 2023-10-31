using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Value
{
[CreateAssetMenu(fileName = "DistanceSkillValue", menuName = "Skill/Value/Distance")]
public class DistanceSkillValue : SkillValue
{
    [SerializeField]
    private SkillPointType pointA;
    [SerializeField]
    private SkillPointType pointB;

    public SkillPointType PointA => pointA;
    public SkillPointType PointB => pointB;
}
}