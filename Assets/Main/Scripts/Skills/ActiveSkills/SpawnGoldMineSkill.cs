using Fusion;
using UnityEngine;

namespace Main.Scripts.Skills.ActiveSkills
{
    public class SpawnGoldMineSkill : ActivePointSkillBase
    {

        [SerializeField]
        private GameObject areaMarker = default!;
        [SerializeField]
        private GameObject mineGold = default!;
        [SerializeField]
        private float radius = 1f;
        
        [Networked]
        private TickTimer timer { get; set; }
        [Networked] 
        private Vector2 position { get; set; }
        [Networked]
        private bool isExecuted { get; set; }
        [Networked]
        private bool isActivated { get; set; }

        private float animationDurationSec = 1f;
        private GameObject? marker;

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
            if (!HasInputAuthority) return;

            if (isActivated)
            {
                if (marker == null)
                {
                    marker = Instantiate(areaMarker, new Vector3(position.x, 0.001f, position.y), areaMarker.transform.rotation);
                }

                if (!marker.activeSelf)
                {
                    marker.SetActive(true);
                }

                marker.transform.position = new Vector3(position.x, 0.001f, position.y);
                marker.transform.localScale = new Vector3(radius, radius, 1);
            }
            else if (marker != null && marker.activeSelf)
            {
                marker.SetActive(false);
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