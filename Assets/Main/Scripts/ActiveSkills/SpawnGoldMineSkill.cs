using Fusion;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.ActiveSkills
{
    public class SpawnGoldMineSkill : ActivePointSkillBase
    {

        [SerializeField]
        private GameObject mineGold = default!;
        
        [Networked]
        private TickTimer timer { get; set; }
        [Networked] 
        private Vector2 position { get; set; }
        [Networked]
        private bool isExecuted { get; set; }
        [Networked]
        private bool isActivated { get; set; }

        private float animationDurationSec = 1f;

        public override bool Activate(PlayerRef owner)
        {
            if (isActivated)
            {
                return false;
            }

            OnWaitingForPointEvent.Invoke(this);
            isActivated = true;
            return true;
        }

        public override bool IsOverrideMove()
        {
            return false;
        }

        public override void ApplyTargetPosition(Vector2 position)
        {
            this.position = position;
        }

        public override void Execute()
        {
            if (timer.ExpiredOrNotRunning(Runner))
            {
                timer = TickTimer.CreateFromSeconds(Runner, animationDurationSec);
                isExecuted = true;
                OnSkillExecutedEvent.Invoke(this);
            }
        }

        public override void Cancel()
        {
            isActivated = false;
            OnSkillCanceledEvent.Invoke(this);
        }


        public override void Render()
        {
            if (isActivated)
            {
                DebugHelper.DrawSphere(new Vector3(position.x, 0, position.y), 0.5f, Color.grey, 1 / 60f);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (isExecuted)
            {
                isActivated = false;
                isExecuted = false;
                Runner.Spawn(mineGold, new Vector3(position.x, 0, position.y));
            }

            if (timer.Expired(Runner))
            {
                timer = default;
                OnSkillFinishedEvent.Invoke(this);
            }
        }
    }
}