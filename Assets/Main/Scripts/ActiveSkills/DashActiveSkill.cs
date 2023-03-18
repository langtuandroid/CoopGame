using System;
using Fusion;
using Main.Scripts.Actions;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.ActiveSkills
{
    public class DashActiveSkill : ActiveSkillBase
    {
        [SerializeField]
        private float distance = 4f;
        [SerializeField]
        private float speed = 20f;

        [SerializeField]
        private Transform dashMovableTargetObject = default!;
        private Movable dashMovableTarget = default!;

        [Networked]
        private TickTimer dashTimer { get; set; }

        private void Awake()
        {
            dashMovableTarget = dashMovableTargetObject.GetComponent<Movable>().ThrowWhenNull();
        }

        private void OnValidate()
        {
            if (!dashMovableTargetObject || !dashMovableTargetObject.TryGetComponent(out dashMovableTarget))
            {
                throw new ArgumentException("Dash movable target not assigned");
            }
        }

        public override bool Activate(PlayerRef owner)
        {
            if (dashTimer.ExpiredOrNotRunning(Runner))
            {
                dashTimer = TickTimer.CreateFromSeconds(Runner, distance / speed);
                OnSkillExecutedEvent.Invoke(this);
                return true;
            }

            return false;
        }

        public override bool IsOverrideMove()
        {
            return dashTimer.IsRunning;
        }

        public override void FixedUpdateNetwork()
        {
            if (dashTimer.IsRunning)
            {
                dashMovableTarget.Move(speed * dashMovableTarget.GetMovingDirection());
            }

            if (dashTimer.Expired(Runner))
            {
                dashTimer = default;
                OnSkillFinishedEvent.Invoke(this);
            }
        }
    }
}