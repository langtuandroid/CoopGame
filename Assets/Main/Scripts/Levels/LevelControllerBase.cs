using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using Main.Scripts.Player;
using Main.Scripts.Player.Data;
using Main.Scripts.Room;
using Main.Scripts.Utils;
using UnityEngine;

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
            GameLoopPhase.DespawnPhase,
            GameLoopPhase.ObjectsSpawnPhase,
            GameLoopPhase.LevelStrategyPhase
        };

        public override void Spawned()
        {
            base.Spawned();
            isLocalInitializedAfterSceneReady = false;
            levelContext.GameLoopManager.Init(Runner.Config.Simulation.TickRate);

            roomManager = RoomManager.Instance.ThrowWhenNull();
            playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
            playersHolder = LevelContext.Instance.ThrowWhenNull().PlayersHolder;
        }

        public void AfterSpawned()
        {
            roomManager.OnPlayerInitializedEvent.AddListener(OnPlayerInitialized);
            roomManager.OnPlayerDisconnectedEvent.AddListener(OnPlayerDisconnected);

            if (HasStateAuthority)
            {
                foreach (var playerRef in Runner.ActivePlayers)
                {
                    if (roomManager.IsPlayerInitialized(playerRef))
                    {
                        OnPlayerInitialized(playerRef);
                    }
                }
            }
        }

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.DespawnPhase:
                    OnDespawnPhase();
                    break;
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

        protected abstract void OnLocalPlayerLoaded();
        
        
        public override void FixedUpdateNetwork()
        {
            if (!isLocalInitializedAfterSceneReady
                && roomManager.IsPlayerInitialized(Runner.LocalPlayer)
                && Runner.IsSceneReady())
            {
                isLocalInitializedAfterSceneReady = true;
                
                OnLocalPlayerLoaded();
            }
            if (IsLevelReady())
            {
                levelContext.GameLoopManager.SimulateLoop();
            }
        }

        protected abstract bool IsLevelReady();

        protected abstract void OnSpawnPhase();

        protected abstract void OnDespawnPhase();

        protected abstract void OnLevelStrategyPhase();
    }
}