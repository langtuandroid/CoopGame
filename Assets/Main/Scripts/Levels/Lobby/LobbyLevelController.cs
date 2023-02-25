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

        private Lazy<PlayerCamera> playerCameraLazy = new(
            () => FindObjectOfType<PlayerCamera>().ThrowWhenNull()
        );
        private PlayerCamera playerCamera => playerCameraLazy.Value;

        public override void Spawned()
        {
            base.Spawned();
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

                    playerController.OnPlayerDeadEvent.AddListener(OnPlayerDead);
                    playerController.OnPlayerStateChangedEvent.AddListener(OnPlayerStateChanged);
                }
            );
        }

        private void OnPlayerDead(PlayerRef playerRef)
        {
            //todo
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerState), playerState, null);
            }
        }

        private void OnAllPlayersReady()
        {
            roomManager.OnAllPlayersReady();
        }

        private void TryShowLevelResults(PlayerRef playerRef)
        {
            if (roomManager.GetLevelResults(roomManager.GetUserId(playerRef)) != null)
            {
                //todo clear levelResults for userId
                playersHolder.Get(playerRef).GetComponent<WindowsController>().SetCurrentWindowType(WindowType.LEVEL_RESULTS);
            }
        }
    }
}