using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.Resources;
using Main.Scripts.Player;
using Main.Scripts.Player.Config;
using Main.Scripts.Tasks;
using Main.Scripts.UI.Windows;
using Main.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Main.Scripts.Levels.Lobby
{
    public class LobbyLevelController : LevelControllerBase
    {
        [SerializeField]
        private PlayerController playerPrefab = null!;
        [SerializeField]
        private PlaceTargetTask readyToStartTask = null!;
        [SerializeField]
        private UIScreenManager screenManager = null!;
        
        private PlayerCamera playerCamera = null!;
        private UIScreenManager uiScreenManager = null!;
        private HeroConfigsBank heroConfigsBank = null!;

        private List<PlayerRef> despawnActions = new();
        private List<SpawnAction> spawnActions = new();
        private bool shouldStartMission;

        public override void Spawned()
        {
            base.Spawned();
            spawnActions.Clear();
            shouldStartMission = false;
            
            playerCamera = PlayerCamera.Instance.ThrowWhenNull();
            uiScreenManager = UIScreenManager.Instance.ThrowWhenNull();
            heroConfigsBank = GlobalResources.Instance.ThrowWhenNull().HeroConfigsBank;
            
            readyToStartTask.OnTaskCheckChangedEvent.AddListener(OnReadyTargetStatusChanged);
            playerDataManager.OnLocalHeroChangedEvent.AddListener(OnLocalHeroChanged);
        }

        public override void Render()
        {
            if (playersHolder.Contains(Runner.LocalPlayer))
            {
                playerCamera.SetTarget(playersHolder.Get(Runner.LocalPlayer).GetComponent<NetworkTransform>().InterpolationTarget.transform);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            readyToStartTask.OnTaskCheckChangedEvent.RemoveListener(OnReadyTargetStatusChanged);
            playerDataManager.OnLocalHeroChangedEvent.RemoveListener(OnLocalHeroChanged);
            
            heroConfigsBank = null!;
            
            base.Despawned(runner, hasState);
        }

        protected override void OnPlayerInitialized(PlayerRef playerRef)
        {
        }

        protected override void OnPlayerDisconnected(PlayerRef playerRef)
        {
            
        }

        protected override void OnLocalPlayerLoaded()
        {
            if (playerDataManager.HasHeroData(Runner.LocalPlayer))
            {
                spawnActions.Add(new SpawnAction {
                    playerRef = Runner.LocalPlayer,
                    spawnPosition = new Vector3(Random.Range(-1,1), 0, Random.Range(-1, 1))
                });
            }
            else
            {
                screenManager.SetScreenType(ScreenType.HERO_PICKER);
            }
        }

        protected override void OnSpawnPhase()
        {
            foreach (var spawnAction in spawnActions)
            {
                SpawnLocalPlayer(spawnAction);
            }
            spawnActions.Clear();
        }

        protected override void OnDespawnPhase()
        {
            foreach (var playerRef in despawnActions)
            {
                if (playersHolder.Contains(playerRef))
                {
                    Runner.Despawn(playersHolder.Get(playerRef).Object);
                }
            }
            despawnActions.Clear();
        }

        protected override void OnLevelStrategyPhase()
        {
            if (shouldStartMission)
            {
                shouldStartMission = false;
                roomManager.OnAllPlayersReady();
            }
        }

        protected override bool IsLevelReady()
        {
            return Runner.IsSceneReady();
        }

        private void OnLocalPlayerStateChanged(
            PlayerRef playerRef,
            PlayerController playerController,
            PlayerState playerState
        )
        {
            switch (playerState)
            {
                case PlayerState.None:
                    break;
                case PlayerState.Despawned:
                    break;
                case PlayerState.Spawning:
                    playerController.Active();
                    break;
                case PlayerState.Active:
                    break;
                case PlayerState.Dead:
                    playerController.Respawn();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerState), playerState, null);
            }
        }

        private void OnReadyTargetStatusChanged(bool isChecked)
        {
            if (!HasStateAuthority) return;
            
            if (isChecked)
            {
                readyToStartTask.OnTaskCheckChangedEvent.RemoveListener(OnReadyTargetStatusChanged);
                shouldStartMission = true;
            }
        }

        private void TryShowLevelResults()
        {
            var playerRef = Runner.LocalPlayer;
            if (!roomManager.IsPlayerInitialized(playerRef))
            {
                return;
            }

            if (roomManager.GetLevelResults(playerRef) != null)
            {
                roomManager.OnLevelResultsShown(playerRef);
                uiScreenManager.SetScreenType(ScreenType.LEVEL_RESULTS);
            }
        }

        private void OnLocalHeroChanged()
        {
            var spawnPosition = Vector3.zero;
            if (playersHolder.Contains(Runner.LocalPlayer))
            {
                spawnPosition = playersHolder.Get(Runner.LocalPlayer).transform.position;
                despawnActions.Add(Runner.LocalPlayer);
            }
            spawnActions.Add(new SpawnAction
            {
                playerRef = Runner.LocalPlayer,
                spawnPosition = spawnPosition
            });
            uiScreenManager.SetScreenType(ScreenType.NONE);
        }

        private void SpawnLocalPlayer(SpawnAction spawnAction)
        {
            Runner.Spawn(
                prefab: playerPrefab,
                position: spawnAction.spawnPosition,
                rotation: Quaternion.identity,
                inputAuthority: spawnAction.playerRef,
                onBeforeSpawned: (networkRunner, playerObject) =>
                {
                    var playerController = playerObject.GetComponent<PlayerController>();

                    playerController.Init(heroConfigsBank.GetHeroConfigKey(playerDataManager.SelectedHeroId));
                    playerController.OnPlayerStateChangedEvent.AddListener(OnLocalPlayerStateChanged);
                }
            );

            TryShowLevelResults();
        }

        private struct SpawnAction
        {
            public PlayerRef playerRef;
            public Vector3 spawnPosition;
        }
    }
}