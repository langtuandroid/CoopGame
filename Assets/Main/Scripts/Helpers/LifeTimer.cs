using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using UnityEngine;

namespace Main.Scripts.Helpers
{
    public class LifeTimer : GameLoopEntity
    {
        [SerializeField]
        private float lifeDurationSec;
        
        [Networked]
        private TickTimer lifeTimer { get; set; }

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.ApplyActionsPhase,
            GameLoopPhase.DespawnPhase
        };

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
            return gameLoopPhases;
        }

        public void SetLifeDuration(float durationSec)
        {
            lifeDurationSec = durationSec;
        }
    }
}