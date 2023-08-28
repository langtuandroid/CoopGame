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
    public abstract class LevelControllerBase : GameLoopEntity, IAfterSpawned
    {
        protected RoomManager roomManager = default!;
        protected PlayerDataManager playerDataManager = default!;
        protected PlayersHolder playersHolder = default!;

        [Networked]
        private NetworkBool isInitialized { get; set; }

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.ObjectsSpawnPhase
        };

        public void AfterSpawned()
        {
            roomManager = RoomManager.Instance.ThrowWhenNull();
            playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
            playersHolder = LevelContext.Instance.ThrowWhenNull().PlayersHolder;

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
        }

        protected abstract void OnPlayerInitialized(PlayerRef playerRef);

        protected abstract void OnPlayerDisconnected(PlayerRef playerRef);
        
        
        public override void FixedUpdateNetwork()
        {
            if (!isInitialized)
            {
                var connectedPlayers = Runner.ActivePlayers;
                foreach (var playerRef in connectedPlayers)
                {
                    if (roomManager.IsPlayerInitialized(playerRef))
                    {
                        OnPlayerInitialized(playerRef);
                    }
                }

                isInitialized = true;
            }
        }

        protected abstract void OnSpawnPhase();
    }
}