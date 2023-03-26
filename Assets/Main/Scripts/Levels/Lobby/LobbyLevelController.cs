using System;
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
        private PlayersHolder playersHolder = default!;
        [SerializeField]
        private PlaceTargetTask readyToStartTask = default!;

        private PlayerCamera playerCamera = default!;
        private UIScreenManager uiScreenManager = default!;

        public override void Spawned()
        {
            base.Spawned();
            playerCamera = PlayerCamera.Instance.ThrowWhenNull();
            uiScreenManager = UIScreenManager.Instance.ThrowWhenNull();
            if (HasStateAuthority)
            {
                readyToStartTask.OnTaskCheckChangedEvent.AddListener(OnReadyTargetStatusChanged);
            }

            playersHolder.OnChangedEvent.AddListener(OnPlayersHolderChanged);
        }

        public override void Render()
        {
            if (playersHolder.Contains(Runner.LocalPlayer))
            {
                playerCamera.SetTarget(playersHolder.Get(Runner.LocalPlayer).transform);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            readyToStartTask.OnTaskCheckChangedEvent.RemoveListener(OnReadyTargetStatusChanged);
        }

        protected override void OnPlayerInitialized(PlayerRef playerRef)
        {
            //todo добавить спавн поинты
            Runner.Spawn(
                prefab: playerPrefab,
                position: Vector3.zero,
                rotation: Quaternion.identity,
                inputAuthority: playerRef,
                onBeforeSpawned: (networkRunner, playerObject) =>
                {
                    var playerController = playerObject.GetComponent<PlayerController>();
                    playerController.ResetState();

                    playerController.OnPlayerStateChangedEvent.AddListener(OnPlayerStateChanged);
                }
            );
        }

        protected override void OnPlayerDisconnected(PlayerRef playerRef)
        {
            if (playersHolder.Contains(playerRef))
            {
                var playerController = playersHolder.Get(playerRef);
                Runner.Despawn(playerController.Object);
                playersHolder.Remove(playerRef);
            }
        }

        private void OnPlayerStateChanged(
            PlayerRef playerRef,
            PlayerController playerController,
            PlayerController.State playerState
        )
        {
            switch (playerState)
            {
                case PlayerController.State.None:
                    break;
                case PlayerController.State.Despawned:
                    break;
                case PlayerController.State.Spawning:
                    playerController.Active();
                    break;
                case PlayerController.State.Active:
                    if (!playersHolder.Contains(playerRef))
                    {
                        playersHolder.Add(playerRef, playerController);
                    }
                    break;
                case PlayerController.State.Dead:
                    OnPlayerDead(playerRef, playerController);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerState), playerState, null);
            }
        }
        
        private void OnPlayerDead(PlayerRef playerRef, PlayerController playerController)
        {
            playerController.ResetState();
        }

        private void OnReadyTargetStatusChanged(bool isChecked)
        {
            if (isChecked)
            {
                readyToStartTask.OnTaskCheckChangedEvent.RemoveListener(OnReadyTargetStatusChanged);
                roomManager.OnAllPlayersReady();
            }
        }

        private void OnPlayersHolderChanged()
        {
            TryShowLevelResults(Runner.LocalPlayer);
        }

        private void TryShowLevelResults(PlayerRef playerRef)
        {
            if (!roomManager.IsPlayerInitialized(playerRef))
            {
                return;
            }
            
            var userId = roomManager.GetUserId(playerRef);
            if (roomManager.GetLevelResults(userId) != null)
            {
                roomManager.OnLevelResultsShown(userId);
                uiScreenManager.SetScreenType(ScreenType.LEVEL_RESULTS);
            }
        }
    }
}