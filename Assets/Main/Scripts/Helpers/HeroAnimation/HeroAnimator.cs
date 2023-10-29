using System;
using UnityEditor.Animations;
using UnityEngine;

namespace Main.Scripts.Helpers.HeroAnimation
{
public class HeroAnimator
{
    private static readonly int MOVE_X_ANIM = Animator.StringToHash("MoveX");
    private static readonly int MOVE_Z_ANIM = Animator.StringToHash("MoveZ");

    private static readonly int PRIMARY_CAST_ANIM = Animator.StringToHash("PrimaryCast");
    private static readonly int PRIMARY_EXECUTE_ANIM = Animator.StringToHash("PrimaryExecute");
    private static readonly int SECONDARY_CAST_ANIM = Animator.StringToHash("SecondaryCast");
    private static readonly int SECONDARY_EXECUTE_ANIM = Animator.StringToHash("SecondaryExecute");
    private static readonly int FIRST_CAST_ANIM = Animator.StringToHash("FirstCast");
    private static readonly int FIRST_EXECUTE_ANIM = Animator.StringToHash("FirstExecute");
    private static readonly int SECOND_CAST_ANIM = Animator.StringToHash("SecondCast");
    private static readonly int SECOND_EXECUTE_ANIM = Animator.StringToHash("SecondExecute");
    private static readonly int THIRD_CAST_ANIM = Animator.StringToHash("ThirdCast");
    private static readonly int THIRD_EXECUTE_ANIM = Animator.StringToHash("ThirdExecute");

    private Animator animator;
    private HeroAnimationType currentAnimType;

    public HeroAnimator(Animator animator)
    {
        this.animator = animator;
    }

    public void SetController(AnimatorController controller)
    {
        animator.runtimeAnimatorController = controller;
    }

    public void SetMoveAnimation(float moveX, float moveZ)
    {
        animator.SetFloat(MOVE_X_ANIM, moveX);
        animator.SetFloat(MOVE_Z_ANIM, moveZ);
    }

    public void StartAnimation(HeroAnimationType animationType, int animationIndex)
    {
        Reset();

        if (animationType == HeroAnimationType.None) return;

        currentAnimType = animationType;

        animator.SetInteger(GetAnimId(currentAnimType), animationIndex);
    }

    public void Reset()
    {
        if (currentAnimType != HeroAnimationType.None)
        {
            animator.SetInteger(GetAnimId(currentAnimType), -1);
            currentAnimType = HeroAnimationType.None;
        }
    }

    private int GetAnimId(HeroAnimationType animationType)
    {
        return animationType switch
        {
            HeroAnimationType.PrimaryCast => PRIMARY_CAST_ANIM,
            HeroAnimationType.PrimaryExecute => PRIMARY_EXECUTE_ANIM,
            HeroAnimationType.SecondaryCast => SECONDARY_CAST_ANIM,
            HeroAnimationType.SecondaryExecute => SECONDARY_EXECUTE_ANIM,
            HeroAnimationType.FirstCast => FIRST_CAST_ANIM,
            HeroAnimationType.FirstExecute => FIRST_EXECUTE_ANIM,
            HeroAnimationType.SecondCast => SECOND_CAST_ANIM,
            HeroAnimationType.SecondExecute => SECOND_EXECUTE_ANIM,
            HeroAnimationType.ThirdCast => THIRD_CAST_ANIM,
            HeroAnimationType.ThirdExecute => THIRD_EXECUTE_ANIM,
            _ => throw new ArgumentOutOfRangeException(nameof(animationType), animationType, null)
        };
    }
}
}