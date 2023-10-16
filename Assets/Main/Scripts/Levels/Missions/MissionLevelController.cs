using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.Resources;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player;
using Main.Scripts.Player.Config;
using Main.Scripts.Scenarios.Missions;
using Main.Scripts.Tasks;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Levels.Missions
{
    public class MissionLevelController : LevelControllerBase
    {
        [SerializeField]
        private PlayerController playerPrefab = null!;
        [SerializeField]
        private KillTargetsCountMissionScenario missionScenario = null!;
        [SerializeField]
        private PlaceTargetTask placeTargetTask = null!;

        private PlayerCamera playerCamera = null!;
        private HeroConfigsBank heroConfigsBank = null!;

        private List<PlayerRef> spawnActions = new();
        private MissionState missionState;

        private HashSet<PlayerRef> playersReady = new();

        private bool isPlayersReady;

        public override void Spawned()
        {
            base.Spawned();
            spawnActions.Clear();
            missionState = MissionState.Loading;
            playersReady.Clear();
            isPlayersReady = false;
            
            playerCamera = PlayerCamera.Instance.ThrowWhenNull();
            heroConfigsBank = GlobalResources.Instance.ThrowWhenNull().HeroConfigsBank;

            if (HasStateAuthority)
            {
                placeTargetTask.OnTaskCheckChangedEvent.AddListener(OnFinishTaskStatus);
                missionScenario.OnScenarioFinishedEvent.AddListener(OnMissionScenarioFinished);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            placeTargetTask.OnTaskCheckChangedEvent.RemoveListener(OnFinishTaskStatus);
            missionScenario.OnScenarioFinishedEvent.RemoveListener(OnMissionScenarioFinished);

            heroConfigsBank = null!;
            
            base.Despawned(runner, hasState);
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
            
        }

        protected override void OnPlayerDisconnected(PlayerRef playerRef)
        {
            
        }

        protected override void OnLocalPlayerLoaded()
        {
            RPC_OnPlayerReady(Runner.LocalPlayer);
        }

        protected override void OnSpawnPhase()
        {
            foreach (var playerRef in spawnActions)
            {
                SpawnLocalPlayer(playerRef);
            }
            spawnActions.Clear();
        }

        protected override void OnDespawnPhase()
        {
            
        }

        protected override void OnLevelStrategyPhase()
        {
            if (!HasStateAuthority) return;
            
            switch (missionState)
            {
                case MissionState.Loading:
                    if (isPlayersReady)
                    {
                        missionState = MissionState.Active;
                        
                        foreach (var playerRef in Runner.ActivePlayers)
                        {
                            RPC_AddSpawnPlayerAction(playerRef);
                        }
                    }
                    break;
                case MissionState.Active:
                    break;
                case MissionState.Failed:
                    OnMissionFailed();
                    break;
                case MissionState.Success:
                    OnMissionSuccess();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override bool IsLevelReady()
        {
            return Runner.IsSceneReady() && isPlayersReady;
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

        private void OnMissionSuccess()
        {
            var levelResults = new Dictionary<PlayerRef, LevelResultsData>();
            //todo use playerDataManager
            foreach (var playerRef in playersHolder.GetKeys())
            {
                levelResults.Add(playerRef, new LevelResultsData
                {
                    IsSuccess = true
                });
            }

            roomManager.OnLevelFinished(levelResults);
        }

        private void OnMissionFailed()
        {
            var levelResults = new Dictionary<PlayerRef, LevelResultsData>();
            foreach (var playerRef in Runner.ActivePlayers)
            {
                levelResults.Add(playerRef, new LevelResultsData
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

                if (missionState == MissionState.Active)
                {
                    //todo переделать логику, чтобы не было конфликтов состояний
                    missionState = MissionState.Success;
                }
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

                    playerController.Init(heroConfigsBank.GetHeroConfigKey(playerDataManager.SelectedHeroId));
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

            if (missionState == MissionState.Active)
            {
                //todo переделать логику, чтобы не было конфликтов состояний
                missionState = MissionState.Failed;
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_OnPlayerReady(PlayerRef playerRefReady)
        {
            if (!HasStateAuthority) return;
            
            playersReady.Add(playerRefReady);

            if (isPlayersReady)
            {
                RPC_AddSpawnPlayerAction(playerRefReady);
                return;
            }
            
            foreach (var playerRef in Runner.ActivePlayers)
            {
                if (!playersReady.Contains(playerRef))
                {
                    return;
                }
            }

            RPC_OnPlayersReady();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_OnPlayersReady()
        {
            isPlayersReady = true;
        }
    }
}