using Fusion;
using Main.Scripts.Core.GameLogic;
using UnityEngine;

namespace Main.Scripts.Helpers
{
    public class LifeTimer : GameLoopEntity
    {
        [SerializeField]
        private float lifeDurationSec;
        
        [Networked]
        private TickTimer lifeTimer { get; set; }

        public override void OnBeforePhysics()
        {
            if (!lifeTimer.IsRunning)
            {
                lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeDurationSec);
            }

            if (lifeTimer.Expired(Runner))
            {
                Runner.Despawn(Object);
            }
        }

        public void SetLifeDuration(float durationSec)
        {
            lifeDurationSec = durationSec;
        }
    }
}