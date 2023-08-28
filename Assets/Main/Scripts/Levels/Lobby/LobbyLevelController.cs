using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Player;
using Main.Scripts.Tasks;
using Main.Scripts.UI.Windows;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Levels.Lobby
{
    public class LobbyLevelController : LevelControllerBase
    {
        [SerializeField]
        private PlayerController playerPrefab = default!;
        [SerializeField]
        private PlaceTargetTask readyToStartTask = default!;
        
        private PlayerCamera playerCamera = default!;
        private UIScreenManager uiScreenManager = default!;

        private List<PlayerRef> spawnActions = new();

        public override void Spawned()
        {
            base.Spawned();
            spawnActions.Clear();
            
            playerCamera = PlayerCamera.Instance.ThrowWhenNull();
            uiScreenManager = UIScreenManager.Instance.ThrowWhenNull();
            readyToStartTask.OnTaskCheckChangedEvent.AddListener(OnReadyTargetStatusChanged);
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
            base.Despawned(runner, hasState);
            readyToStartTask.OnTaskCheckChangedEvent.RemoveListener(OnReadyTargetStatusChanged);
        }

        protected override void OnPlayerInitialized(PlayerRef playerRef)
        {
            if (!HasStateAuthority) return;
            //todo добавить спавн поинты
            RPC_AddSpawnPlayerAction(playerRef);
        }

        protected override void OnPlayerDisconnected(PlayerRef playerRef)
        {
            
        }

        protected override void OnSpawnPhase()
        {
            //todo добавить спавн поинты
            foreach (var playerRef in spawnActions)
            {
                SpawnLocalPlayer(playerRef);
            }
            spawnActions.Clear();
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
                roomManager.OnAllPlayersReady();
            }
        }

        private void TryShowLevelResults()
        {
            var playerRef = Runner.LocalPlayer;
            if (!roomManager.IsPlayerInitialized(playerRef))
            {
                return;
            }

            var userId = playerDataManager.GetUserId(playerRef);
            if (roomManager.GetLevelResults(userId) != null)
            {
                roomManager.OnLevelResultsShown(userId);
                uiScreenManager.SetScreenType(ScreenType.LEVEL_RESULTS);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_AddSpawnPlayerAction([RpcTarget] PlayerRef playerRef)
        {
            spawnActions.Add(playerRef);
        }

        private void SpawnLocalPlayer(PlayerRef playerRef)
        {
            Runner.Spawn(
                prefab: playerPrefab,
                position: Vector3.zero,
                rotation: Quaternion.identity,
                inputAuthority: playerRef,
                onBeforeSpawned: (networkRunner, playerObject) =>
                {
                    var playerController = playerObject.GetComponent<PlayerController>();

                    playerController.OnPlayerStateChangedEvent.AddListener(OnLocalPlayerStateChanged);
                }
            );

            TryShowLevelResults();
        }
    }
}