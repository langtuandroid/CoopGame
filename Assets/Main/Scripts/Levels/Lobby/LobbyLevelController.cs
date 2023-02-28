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

        public override void Spawned()
        {
            base.Spawned();
            playerCamera = FindObjectOfType<PlayerCamera>().ThrowWhenNull();
            if (HasStateAuthority)
            {
                readyToStartTask.OnTaskCompleted.AddListener(OnAllPlayersReady);
            }
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
            readyToStartTask.OnTaskCompleted.RemoveListener(OnAllPlayersReady);
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
                    playerController.Reset();

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

        private void OnPlayerDead(PlayerRef playerRef, PlayerController playerController)
        {
            //todo
            playerController.OnPlayerStateChangedEvent.RemoveListener(OnPlayerStateChanged);
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
                    playersHolder.Add(playerRef, playerController);
                    TryShowLevelResults(playerRef);
                    break;
                case PlayerController.State.Dead:
                    OnPlayerDead(playerRef, playerController);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerState), playerState, null);
            }
        }

        private void OnAllPlayersReady()
        {
            roomManager.OnAllPlayersReady();
        }

        private void TryShowLevelResults([RpcTarget] PlayerRef playerRef)
        {
            var userId = roomManager.GetUserId(playerRef);
            if (roomManager.GetLevelResults(userId) != null)
            {
                roomManager.OnLevelResultsShown(userId);
                playersHolder.Get(playerRef).GetComponent<WindowsController>().SetCurrentWindowType(WindowType.LEVEL_RESULTS);
            }
        }
    }
}