using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
[CreateAssetMenu(fileName = "StartOwnerAnimationAction", menuName = "Skill/Action/StartOwnerAnimation")]
public class StartOwnerAnimationSkillAction : SkillActionBase
{
    [SerializeField]
    [Min(0)]
    private int animationIndex;

    public int AnimationIndex => animationIndex;
}
}