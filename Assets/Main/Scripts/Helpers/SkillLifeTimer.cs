using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using UnityEngine;

namespace Main.Scripts.Helpers
{
    public class SkillLifeTimer : GameLoopEntity
    {
        [SerializeField]
        private float lifeDurationSec;

        private TickTimer lifeTimer;

        private GameLoopPhase[] localGameLoopPhases;
        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.ApplyActionsPhase,
            GameLoopPhase.DespawnPhase
        };

        public override void Spawned()
        {
            base.Spawned();
            lifeTimer = default;
            localGameLoopPhases = HasStateAuthority ? gameLoopPhases : Array.Empty<GameLoopPhase>();
        }

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.ApplyActionsPhase:
                    if (!lifeTimer.IsRunning)
                    {
                        lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeDurationSec);
                    }
                    break;
                case GameLoopPhase.DespawnPhase:
                    if (lifeTimer.Expired(Runner))
                    {
                        Runner.Despawn(Object);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return localGameLoopPhases;
        }

        public void SetLifeDuration(float durationSec)
        {
            lifeDurationSec = durationSec;
        }
    }
}