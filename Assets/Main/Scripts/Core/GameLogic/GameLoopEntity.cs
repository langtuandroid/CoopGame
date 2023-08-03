using Fusion;
using Main.Scripts.Levels;
using Main.Scripts.Utils;

namespace Main.Scripts.Core.GameLogic
{
    public class GameLoopEntity : NetworkBehaviour, GameLoopListener
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

        public virtual void OnSyncTransformBeforeAll()
        {
        }

        public virtual void OnInputPhase()
        {
        }

        public virtual void OnBeforePhysics()
        {
        }

        public virtual void OnBeforePhysicsStep()
        {
        }

        public virtual void OnAfterPhysicsStep()
        {
        }

        public virtual void OnAfterPhysicsSteps()
        {
        }
        
        public virtual void OnSyncTransformAfterAll()
        {
        }
    }
}