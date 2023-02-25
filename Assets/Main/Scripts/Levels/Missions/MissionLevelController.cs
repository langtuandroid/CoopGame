using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player;
using Main.Scripts.Player.Data;
using Main.Scripts.Tasks;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Levels.Missions
{
    public class MissionLevelController : LevelControllerBase
    {
        [SerializeField]
        private PlayerController playerPrefab = default!;
        [SerializeField]
        private PlayersHolder playersHolder = default!;
        [SerializeField]
        private PlaceTargetTask placeTargetTask = default!;

        private PlayerCamera playerCamera = default!;

        public override void Spawned()
        {
            base.Spawned();
            playerCamera = FindObjectOfType<PlayerCamera>().ThrowWhenNull();

            if (HasStateAuthority)
            {
                placeTargetTask.OnTaskCompleted.AddListener(OnPlaceTargetTaskCompleted);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            placeTargetTask.OnTaskCompleted.RemoveListener(OnPlaceTargetTaskCompleted);
        }

        public override void Render()
        {
            if (playersHolder.Contains(Runner.LocalPlayer))
            {
                playerCamera.SetTarget(playersHolder.Get(Runner.LocalPlayer).transform);
            }
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
                    break;
                case PlayerController.State.Dead:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerState), playerState, null);
            }
        }

        private void OnPlayerDead(PlayerRef deadPlayerRef)
        {
            foreach (var playerRef in playersHolder.GetKeys())
            {
                var playerController = playersHolder.Get(playerRef);
                if (playerController.state != PlayerController.State.Dead)
                {
                    return;
                }
            }

            OnMissionFailed();
        }

        private void OnMissionFailed()
        {
            var levelResults = new Dictionary<UserId, LevelResultsData>();
            foreach (var playerRef in playersHolder.GetKeys())
            {
                levelResults.Add(roomManager.GetUserId(playerRef), new LevelResultsData
                {
                    IsSuccess = false,
                    Experience = 50
                });
            }

            roomManager.OnLevelFinished(levelResults);
        }

        private void OnPlaceTargetTaskCompleted()
        {
            var levelResults = new Dictionary<UserId, LevelResultsData>();
            foreach (var playerRef in playersHolder.GetKeys())
            {
                levelResults.Add(roomManager.GetUserId(playerRef), new LevelResultsData
                {
                    IsSuccess = true,
                    Experience = 200
                });
            }

            roomManager.OnLevelFinished(levelResults);
        }
    }
}