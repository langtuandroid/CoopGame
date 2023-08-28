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
        private KillTargetsCountMissionScenario missionScenario = default!;
        [SerializeField]
        private PlaceTargetTask placeTargetTask = default!;

        private PlayerCamera playerCamera = default!;

        private List<PlayerRef> spawnActions = new();

        public override void Spawned()
        {
            base.Spawned();
            spawnActions.Clear();
            
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
                playerCamera.SetTarget(playersHolder.Get(Runner.LocalPlayer).GetComponent<NetworkTransform>().InterpolationTarget.transform);
            }
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
                    RPC_OnPlayerStateDead();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerState), playerState, null);
            }
        }

        private void OnMissionFailed()
        {
            var levelResults = new Dictionary<UserId, LevelResultsData>();
            foreach (var playerRef in playersHolder.GetKeys())
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
                foreach (var playerRef in playersHolder.GetKeys())
                {
                    levelResults.Add(playerDataManager.GetUserId(playerRef), new LevelResultsData
                    {
                        IsSuccess = true
                    });
                }

                roomManager.OnLevelFinished(levelResults);
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
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_OnPlayerStateDead()
        {
            foreach (var playerRef in playersHolder.GetKeys())
            {
                var player = playersHolder.Get(playerRef);
                if (player.GetPlayerState() != PlayerState.Dead)
                {
                    return;
                }
            }

            OnMissionFailed();
        }
    }
}