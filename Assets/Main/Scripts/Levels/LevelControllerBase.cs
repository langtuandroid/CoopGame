using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Player;
using Main.Scripts.Player.Data;
using Main.Scripts.Room;
using Main.Scripts.Utils;

namespace Main.Scripts.Levels
{
    public abstract class LevelControllerBase : GameLoopEntityNetworked, IAfterSpawned
    {
        protected RoomManager roomManager = default!;
        protected PlayerDataManager playerDataManager = default!;
        protected PlayersHolder playersHolder = default!;

        private bool isLocalInitializedAfterSceneReady;


        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.ObjectsSpawnPhase,
            GameLoopPhase.LevelStrategyPhase
        };

        public void AfterSpawned()
        {
            isLocalInitializedAfterSceneReady = false;
            
            roomManager = RoomManager.Instance.ThrowWhenNull();
            playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
            playersHolder = LevelContext.Instance.ThrowWhenNull().PlayersHolder;
            levelContext.GameLoopManager.Init(Runner.Config.Simulation.TickRate);

            roomManager.OnPlayerInitializedEvent.AddListener(OnPlayerInitialized);
            roomManager.OnPlayerDisconnectedEvent.AddListener(OnPlayerDisconnected);
        }

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.ObjectsSpawnPhase:
                    OnSpawnPhase();
                    break;
                case GameLoopPhase.LevelStrategyPhase:
                    OnLevelStrategyPhase();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
            
        }

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return gameLoopPhases;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            roomManager.OnPlayerInitializedEvent.RemoveListener(OnPlayerInitialized);
            roomManager.OnPlayerDisconnectedEvent.RemoveListener(OnPlayerDisconnected);
        }

        protected abstract void OnPlayerInitialized(PlayerRef playerRef);

        protected abstract void OnPlayerDisconnected(PlayerRef playerRef);

        protected abstract void OnLocalPlayerLoaded(PlayerRef playerRef);
        
        
        public override void FixedUpdateNetwork()
        {
            if (!isLocalInitializedAfterSceneReady
                && roomManager.IsPlayerInitialized(Runner.LocalPlayer)
                && Runner.IsSceneReady())
            {
                isLocalInitializedAfterSceneReady = true;
                
                OnLocalPlayerLoaded(Runner.LocalPlayer);
            }
            if (IsLevelReady())
            {
                levelContext.GameLoopManager.SimulateLoop();
            }
        }

        protected abstract bool IsLevelReady();

        protected abstract void OnSpawnPhase();

        protected abstract void OnLevelStrategyPhase();
    }
}