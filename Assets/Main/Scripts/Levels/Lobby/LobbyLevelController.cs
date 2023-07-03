using System;
using Fusion;
using Main.Scripts.Player;
using Main.Scripts.Player.InputSystem;
using Main.Scripts.Tasks;
using Main.Scripts.UI.Windows;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Levels.Lobby
{
    public class LobbyLevelController : LevelControllerBase
    {
        [SerializeField]
        private InputController inputController = default!;
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
                playerCamera.SetTarget(playersHolder.Get(Runner.LocalPlayer).GetComponent<NetworkTransform>().InterpolationTarget.transform);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            readyToStartTask.OnTaskCheckChangedEvent.RemoveListener(OnReadyTargetStatusChanged);
            playersHolder.OnChangedEvent.RemoveListener(OnPlayersHolderChanged);
        }

        protected override void OnPlayerInitialized(PlayerRef playerRef)
        {
            //todo добавить спавн поинты
            Runner.Spawn(
                prefab: playerPrefab,
                position: Vector3.zero,
                rotation: Quaternion.identity,
                onBeforeSpawned: (networkRunner, playerObject) =>
                {
                    var playerController = playerObject.GetComponent<PlayerController>();
                    playerController.SetOwnerRef(playerRef);

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
                    if (!playersHolder.Contains(playerRef))
                    {
                        playersHolder.Add(playerRef, playerController);
                        
                        Runner.Spawn(
                            prefab: inputController,
                            position: Vector3.zero,
                            rotation: Quaternion.identity,
                            inputAuthority: playerRef,
                            onBeforeSpawned: (networkRunner, playerObject) =>
                            { }
                        );
                    }
                    break;
                case PlayerState.Dead:
                    OnPlayerDead(playerRef, playerController);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerState), playerState, null);
            }
        }
        
        private void OnPlayerDead(PlayerRef playerRef, PlayerController playerController)
        {
            playerController.Respawn();
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
            TryShowLevelResults();
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
    }
}