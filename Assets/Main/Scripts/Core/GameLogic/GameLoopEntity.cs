using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Levels;
using Main.Scripts.Utils;

namespace Main.Scripts.Core.GameLogic
{
    public abstract class GameLoopEntity : NetworkBehaviour, GameLoopListener
    {
        protected LevelContext levelContext { get; private set; } = default!;

        public override void Spawned()
        {
            base.Spawned();
            
            levelContext = LevelContext.Instance.ThrowWhenNull();

            levelContext.GameLoopManager.AddListener(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);

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