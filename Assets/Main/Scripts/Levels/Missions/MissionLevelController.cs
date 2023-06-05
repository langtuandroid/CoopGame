using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player;
using Main.Scripts.Player.Data;
using Main.Scripts.Scenarios.Missions;
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
        private KillTargetsCountMissionScenario missionScenario = default!;
        [SerializeField]
        private PlaceTargetTask placeTargetTask = default!;

        private PlayerCamera playerCamera = default!;

        [Networked, Capacity(16)]
        private NetworkDictionary<UserId, bool> playersProgress => default; //todo поддержать сохранение прогресса текущей миссии для игроков

        public override void Spawned()
        {
            base.Spawned();
            playerCamera = PlayerCamera.Instance.ThrowWhenNull();

            if (HasStateAuthority)
            {
                placeTargetTask.OnTaskCheckChangedEvent.AddListener(OnFinishTaskStatus);
                missionScenario.OnScenarioFinishedEvent.AddListener(OnMissionScenarioFinished);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            placeTargetTask.OnTaskCheckChangedEvent.RemoveListener(OnFinishTaskStatus);
            missionScenario.OnScenarioFinishedEvent.RemoveListener(OnMissionScenarioFinished);
        }

        public override void Render()
        {
            if (playersHolder.Contains(Runner.LocalPlayer))
            {
                playerCamera.SetTarget(playersHolder.Get(Runner.LocalPlayer).GetComponent<NetworkRigidbody>().InterpolationTarget.transform);
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

        private void OnPlayerDead(PlayerRef deadPlayerRef, PlayerController playerController)
        {
            foreach (var playerRef in playersHolder.GetKeys())
            {
                var player = playersHolder.Get(playerRef);
                if (player.state != PlayerController.State.Dead)
                {
                    return;
                }
            }

            OnMissionFailed();
        }

        private void OnMissionFailed()
        {
            var levelResults = new Dictionary<UserId, LevelResultsData>();
            foreach (var playerRef in playersHolder.GetKeys(false))
            {
                levelResults.Add(playerDataManager.GetUserId(playerRef), new LevelResultsData
                {
                    IsSuccess = false
                });
            }

            roomManager.OnLevelFinished(levelResults);
        }

        private void OnMissionScenarioFinished()
        {
            missionScenario.OnScenarioFinishedEvent.RemoveListener(OnMissionScenarioFinished);
            
            if (HasStateAuthority)
            {
                OnFinishTaskStatus(placeTargetTask.IsTargetChecked);
            }
        }

        private void OnFinishTaskStatus(bool isChecked)
        {
            if (missionScenario.IsFinished && isChecked)
            {
                placeTargetTask.OnTaskCheckChangedEvent.RemoveListener(OnFinishTaskStatus);

                var levelResults = new Dictionary<UserId, LevelResultsData>();
                foreach (var playerRef in playersHolder.GetKeys(false))
                {
                    levelResults.Add(playerDataManager.GetUserId(playerRef), new LevelResultsData
                    {
                        IsSuccess = true
                    });
                }

                roomManager.OnLevelFinished(levelResults);
            }
        }
    }
}