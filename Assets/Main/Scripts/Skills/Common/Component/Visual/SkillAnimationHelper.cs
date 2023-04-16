using Fusion;
using UnityEngine;
using WebSocketSharp;

namespace Main.Scripts.Skills.Common.Component.Visual
{
    public class SkillAnimationHelper: NetworkBehaviour
    {
        [SerializeField]
        private string onSkillActionAnimationTriggerName = "";
        [SerializeField]
        private string onSkillFinishAnimationTriggerName = "";

        private NetworkMecanimAnimator networkAnimator = default!;
        private SkillComponent skillComponent = default!;

        private void Awake()
        {
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            skillComponent = GetComponent<SkillComponent>();
            skillComponent.OnActionEvent.AddListener(OnSkillAction);
            skillComponent.OnFinishEvent.AddListener(OnSkillFinish);
            networkAnimator.Animator.keepAnimatorStateOnDisable = false;
        }

        private void OnDestroy()
        {
            skillComponent.OnActionEvent.RemoveListener(OnSkillAction);
            skillComponent.OnFinishEvent.RemoveListener(OnSkillFinish);
        }

        private void OnSkillAction()
        {
            if (!onSkillActionAnimationTriggerName.IsNullOrEmpty())
            {
                networkAnimator.SetTrigger(onSkillActionAnimationTriggerName);
            }
        }

        private void OnSkillFinish()
        {
            if (!onSkillFinishAnimationTriggerName.IsNullOrEmpty())
            {
                networkAnimator.SetTrigger(onSkillFinishAnimationTriggerName);
            }
        }
    }
}