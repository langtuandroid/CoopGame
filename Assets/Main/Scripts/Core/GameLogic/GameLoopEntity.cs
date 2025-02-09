using System.Collections.Generic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Levels;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Core.GameLogic
{
    public abstract class GameLoopEntity : MonoBehaviour, GameLoopListener
    {
        protected LevelContext levelContext { get; private set; } = default!;

        public void Start()
        {
            levelContext = LevelContext.Instance.ThrowWhenNull();

            levelContext.GameLoopManager.AddListener(this);
        }

        public void OnDestroy()
        {
            if (levelContext != null)
            {
                levelContext.GameLoopManager.RemoveListener(this);
                levelContext = default!;
            }
        }

        public abstract void OnGameLoopPhase(GameLoopPhase phase);
        public abstract IEnumerable<GameLoopPhase> GetSubscribePhases();
    }
}